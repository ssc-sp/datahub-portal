using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Security;
using Datahub.Portal.Pages.Workspace.Storage.ResourcePages;

namespace Datahub.Infrastructure.Services.Storage;

public class AWSCloudStorageManager : ICloudStorageManager
{
	private readonly string _containerName;
	private readonly string _accessKeyId;
	private readonly string _secretAccessKey;
	private readonly string _region;
	private readonly string _bucketName;

	public AWSCloudStorageManager(string containerName, string accessKeyId, string secretAccessKey, string region, string bucketName)
	{
		_containerName = containerName;
		_accessKeyId = accessKeyId;
		_secretAccessKey = secretAccessKey;
		_region = region;
		_bucketName = bucketName;
	}

	private async Task TestConnection()
	{
		using var s3Client = GetClient();
		var request = new ListObjectsV2Request()
		{
			BucketName = _bucketName,
			MaxKeys = 1
		};
		await s3Client.ListObjectsV2Async(request);
	}

	public async Task<List<string>> GetContainersAsync()
	{
		await TestConnection();
		return await Task.FromResult(new List<string>() { _containerName });
	}

	public async Task<DfsPage> GetDfsPagesAsync(string container, string folderPath, string? continuationToken = null)
	{
		var folders = new List<string>();
		var files = new List<FileMetaData>();

		// correct folder path
		folderPath = ToAWSFolder(folderPath);

		// ignore the container
		using var s3Client = GetClient();

		var request = new ListObjectsV2Request()
		{
			BucketName = _bucketName
		};

		ListObjectsV2Response response;
		do
		{
			response = await s3Client.ListObjectsV2Async(request);
			foreach (S3Object entry in response.S3Objects)
			{
				var (belongsToFolder, isFolder, relativePath) = AnalyseFolderItem(folderPath, entry.Key);

				if (!belongsToFolder || string.IsNullOrEmpty(relativePath))
				{
					continue;
				}

				if (isFolder)
				{
					folders.Add(RemoveSlash(entry.Key));
					continue;
				}

                FileMetaData fileMetaData = new()
                {
                    id = entry.ETag,
                    name = relativePath,
                    lastmodifiedts = entry.LastModified,
                    filesize = entry.Size.ToString()
                };

				files.Add(fileMetaData);
			}

			request.ContinuationToken = response.NextContinuationToken;

		} while (response.IsTruncated);

		return new DfsPage(folders, files, continuationToken!);
	}

	public async Task<bool> CreateFolderAsync(string container, string currentWorkingDirectory, string folderName)
	{
		using var s3Client = GetClient();
		try
		{
			if (string.IsNullOrEmpty(folderName))
				return false;

			var request = new PutObjectRequest()
			{
				BucketName = _bucketName,
				Key = ToAWSFolder(folderName),
				InputStream = new MemoryStream()
			};
			await s3Client.PutObjectAsync(request);

			return true;
		}
		catch
		{
			return false;
		}
	}

	public Task<bool> DeleteFileAsync(string container, string filePath)
	{
		return DeleteObjectAsync(filePath);
	}

	public Task<bool> DeleteFolderAsync(string container, string folderPath)
	{
		return DeleteObjectAsync(ToAWSFolder(folderPath));
	}

	public Task<Uri> DownloadFileAsync(string container, string filePath)
	{
		using var s3Client = GetClient();

		var urlRequest = new GetPreSignedUrlRequest
		{
			BucketName = _bucketName,
			Key = filePath,
			Expires = DateTime.UtcNow.AddDays(1),
			Verb = HttpVerb.GET
		};

		var url = s3Client.GetPreSignedURL(urlRequest);
		Uri uri = new Uri(url);

		return Task.FromResult(uri);
	}

	public async Task<bool> FileExistsAsync(string container, string filePath)
	{
		using var s3Client = GetClient();
		try
		{
			var response = await s3Client.GetObjectAsync(new GetObjectRequest()
			{
				BucketName = _bucketName,
				Key = filePath
			});
			return true;
		}
		catch (Amazon.S3.AmazonS3Exception ex)
		{
			if (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
				return false;

			//status wasn't not found, so throw the exception
			throw;
		}
	}

	public Task<Uri> GenerateSasTokenAsync(string container, int days)
	{
		// not supported
		return Task.FromResult(new Uri("http://none"));
	}

	public Task<StorageMetadata> GetStorageMetadataAsync(string container)
	{
		StorageMetadata metadata = new()
		{
			Container = container
		};
		return Task.FromResult(metadata);
	}

	public Task<bool> RenameFileAsync(string container, string oldFilePath, string newFilePath)
	{
		// not supported for now..
		return Task.FromResult(false);
	}

	public bool AzCopyEnabled => false;
	public bool DatabrickEnabled => true;

	public CloudStorageProviderType ProviderType => CloudStorageProviderType.AWS;

	public string DisplayName => _containerName;

	private const long MaxFileSize = 10 * 1024 * 1024 * 1024L; // 10GB

	public async Task<bool> UploadFileAsync(string container, FileMetaData file, Action<long> progess)
	{
		using var s3Client = GetClient();
		try
		{
			var stream = file.BrowserFile.OpenReadStream(MaxFileSize);

			var transferUtility = new TransferUtility(s3Client);

			var fullPath = IsRoot(file.folderpath) ? file.filename : $"{file.folderpath}{file.filename}";
			await transferUtility.UploadAsync(stream, _bucketName, fullPath);

			return true;
		}
		catch
		{
			return false;
		}
	}

	private AmazonS3Client GetClient() => new(_accessKeyId, _secretAccessKey, RegionEndpoint.GetBySystemName(_region));

	private async Task<bool> DeleteObjectAsync(string filePath)
	{
		using var s3Client = GetClient();
		try
		{
			var request = new DeleteObjectRequest()
			{
				BucketName = _bucketName,
				Key = filePath
			};
			var response = await s3Client.DeleteObjectAsync(request);
			return true;
		}
		catch
		{
			return false;
		}
	}

	static (bool Belongs, bool IsFolder, string relativePath) AnalyseFolderItem(string folderPath, string path)
	{
		var isRoot = IsRoot(folderPath);
		if (!isRoot && !path.StartsWith(folderPath, StringComparison.OrdinalIgnoreCase))
		{
			return (false, false, path);
		}

		var pathLength = path.Length;
		var relativeStart = isRoot ? 0 : folderPath.Length;

		for (var i = relativeStart; i < pathLength; i++)
		{
			if (path[i] == '/')
			{
				var isLastChar = i == pathLength - 1;
				return (isLastChar, isLastChar, path.Substring(relativeStart, pathLength - relativeStart - 1));
			}
		}

		return (true, false, path[relativeStart..]);
	}

	static string ToAWSFolder(string path)
	{
		if (string.IsNullOrEmpty(path))
			return "/";

		if (!path.EndsWith("/"))
			return $"{path}/";

		return path;
	}

	static string RemoveSlash(string path) => path.EndsWith('/') ? path[..^1] : path;

	static bool IsRoot(string path) => string.IsNullOrEmpty(path) || path == "/";

	public List<(string Placeholder, string Replacement)> GetSubstitutions(string projectAcronym, CloudStorageContainer container)
	{
		return new List<(string, string)>
		{
			(ResourceSubstitutions.ProjectAcronym, projectAcronym),
			(ResourceSubstitutions.AWSS3Bucket, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_BucketName)),
			(ResourceSubstitutions.AWSAccessKey, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_AccesKeyId)),
			(ResourceSubstitutions.AWSAccessKeySecret, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_AccessKeySecret)),
			(ResourceSubstitutions.AWSRegion, KeyVaultUserService.GetSecretNameForStorage(container.Id.Value, CloudStorageHelpers.AWS_Region))
		};
	}
}
