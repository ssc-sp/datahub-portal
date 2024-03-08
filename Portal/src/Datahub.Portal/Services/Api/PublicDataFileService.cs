using System.Net;
using Datahub.Application.Services.Notification;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Core.Services.Api;
using Datahub.Core.Services.Metadata;
using Datahub.Core.Services.Notification;
using Datahub.Core.Services.Storage;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Portal.Templates;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace Datahub.Portal.Services.Api;

public class PublicFileSharingConfiguration
{
	public string OpenDataApprovalPdfBaseUrl { get; set; }
	public string OpenDataApprovalPdfFormIdParam { get; set; }
	public string PublicFileSharingDomain { get; set; }
	public string OpenDataApproverName { get; set; }
	public string OpenDataApproverEmail { get; set; }
	public string OpenDataApproverEmailSubject { get; set; } = "New Approval Requested / Nouvelle approbation demand�e";
}

public class PublicDataFileService : IPublicDataFileService
{
	private readonly DatahubProjectDBContext _projectDbContext;
	private readonly DataRetrievalService dataRetrievalService;
	private readonly ILogger<IPublicDataFileService> _logger;
	private readonly IMetadataBrokerService _metadataService;
	private readonly IDatahubAuditingService _datahubAuditingService;
	private readonly IEmailNotificationService _emailNotificationService;
	private readonly PublicFileSharingConfiguration _config;

	public static readonly string PUBLIC_FILE_SHARING_CONFIG_ROOT_KEY = "PublicFileSharing";

	public PublicDataFileService(
		DatahubProjectDBContext projectDbContext,
		DataRetrievalService dataRetrievalService,
		ILogger<IPublicDataFileService> logger,
		IDatahubAuditingService datahubAuditingService,
		IMetadataBrokerService metadataService,
		IEmailNotificationService emailNotificationService,
		IConfiguration config
	)
	{
		_projectDbContext = projectDbContext;
		this.dataRetrievalService = dataRetrievalService;
		_logger = logger;
		_datahubAuditingService = datahubAuditingService;
		_metadataService = metadataService;
		_emailNotificationService = emailNotificationService;
		_config = new();
		config.Bind(PUBLIC_FILE_SHARING_CONFIG_ROOT_KEY, _config);
	}

	public async Task CreateDataSharingRequest(FileMetaData fileMetaData, string projectCode, User requestingUser, bool openDataRequest = false)
	{
		if (fileMetaData == null || requestingUser == null)
		{
			_logger.LogError($"Null user {requestingUser} or file {fileMetaData}");
			return;
		}

		if (!Guid.TryParse(fileMetaData.fileid, out Guid fileId))
		{
			_logger.LogError($"Invalid file id: {fileMetaData.fileid}");
			return;
		}

		var existingFile = await _projectDbContext.SharedDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
		if (existingFile != null)
		{
			_logger.LogError($"File {fileId} already has a sharing record");
			return;
		}

		if (openDataRequest)
		{
			_ = Int64.TryParse(fileMetaData.filesize, out Int64 fileSize);

			var shareRequest = new OpenDataSharedFile
			{
				IsOpenDataRequest_FLAG = true,
				File_ID = fileId,
				Filename_TXT = fileMetaData.filename,
				FolderPath_TXT = fileMetaData.folderpath,
				ProjectCode_CD = projectCode?.ToLowerInvariant(),
				RequestingUser_ID = requestingUser.Id,
				RequestedDate_DT = DateTime.UtcNow
			};

			var dbResult = await _projectDbContext.OpenDataSharedFiles.AddAsync(shareRequest);
		}
		else
		{
			var shareRequest = new SharedDataFile()
			{
				File_ID = fileId,
				Filename_TXT = fileMetaData.filename,
				FolderPath_TXT = fileMetaData.folderpath,
				ProjectCode_CD = projectCode?.ToLowerInvariant(),
				RequestingUser_ID = requestingUser.Id,
				RequestedDate_DT = DateTime.UtcNow
			};

			var dbResult = await _projectDbContext.SharedDataFiles.AddAsync(shareRequest);
		}

		var numSaved = await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);

		_logger.LogInformation($"Wrote file share record for {fileId} - {numSaved} records written to database");
	}

	public async Task<string> CreateExternalOpenDataSharing(int approvalFormId, string fileId, string fileName, string fileUrl, string contactEmail, string projectCode)
	{
		var shareRequest = new OpenDataSharedFile
		{
			IsOpenDataRequest_FLAG = true,
			File_ID = Guid.Parse(fileId),
			Filename_TXT = fileName,
			FolderPath_TXT = "",
			ProjectCode_CD = projectCode?.ToLowerInvariant(),
			RequestingUser_ID = contactEmail,
			RequestedDate_DT = DateTime.UtcNow,
			MetadataCompleted_FLAG = true,
			FileUrl_TXT = fileUrl,
			ApprovalForm_ID = approvalFormId
		};

		await _projectDbContext.OpenDataSharedFiles.AddAsync(shareRequest);

		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService, true);

		var ub = new UriBuilder(_config.PublicFileSharingDomain);
		ub.Path = $"/share/opendata/{fileId}";

		return ub.ToString();
	}

	public async Task<Uri> DownloadPublicUrlSharedFile(Guid fileId, IPAddress ipAddress = null)
	{
		var publicFile = await LoadPublicUrlSharedFileInfo(fileId);

		if (publicFile == null)
		{
			_logger.LogError($"File not found: {fileId}");
			//TODO an exception instead of returning null
			return await Task.FromResult<Uri>(null);
		}

		if (!publicFile.ApprovedDate_DT.HasValue || publicFile.ApprovedDate_DT > DateTime.UtcNow)
		{
			_logger.LogError($"File not approved for public: {fileId}");
			return await Task.FromResult<Uri>(null);
		}

		if (!publicFile.PublicationDate_DT.HasValue || publicFile.PublicationDate_DT > DateTime.UtcNow)
		{
			_logger.LogError($"File {fileId} is not yet published (publication: {publicFile.PublicationDate_DT?.ToShortDateString()})");
			return await Task.FromResult<Uri>(null);
		}

		if (publicFile.ExpirationDate_DT.HasValue && publicFile.ExpirationDate_DT < DateTime.UtcNow)
		{
			_logger.LogError($"File {fileId} is no longer available (expiration date: {publicFile.ExpirationDate_DT?.ToShortDateString()})");
			return await Task.FromResult<Uri>(null);
		}

		return await DoDownloadFile(publicFile, anonymous: true, ipAddress: ipAddress);
	}

	public async Task<Uri> DoDownloadFile(SharedDataFile publicFile, bool anonymous = false, IPAddress ipAddress = null)
	{
		var fileMetadata = new FileMetaData()
		{
			filename = publicFile.Filename_TXT,
			name = publicFile.Filename_TXT,
			folderpath = publicFile.FolderPath_TXT
		};

		// audit download file
		if (ipAddress != null)
		{
			await _datahubAuditingService.TrackDataEvent(publicFile.Filename_TXT, publicFile.GetType().Name, AuditChangeType.Download, anonymous, ("ipAddress", ipAddress.ToString()));
		}
		else
		{
			await _datahubAuditingService.TrackDataEvent(publicFile.Filename_TXT, publicFile.GetType().Name, AuditChangeType.Download, anonymous);
		}

		if (publicFile.IsProjectBased)
		{
			return await dataRetrievalService.GetUserDelegationSasBlob(DataRetrievalService.DEFAULT_CONTAINER_NAME, fileMetadata.filename, publicFile.ProjectCode_CD.ToLowerInvariant());
		}
		else
		{
			return await dataRetrievalService.DownloadFile(DataRetrievalService.DEFAULT_CONTAINER_NAME, fileMetadata, null);
		}
	}

	private System.Linq.Expressions.Expression<Func<SharedDataFile, bool>> GenerateSharingRequestsAwaitingApprovalCondition(string projectCode)
	{
		return f => f.ProjectCode_CD != null
					&& f.ProjectCode_CD.ToLower() == projectCode.ToLower()
					&& f.SubmittedDate_DT.HasValue
					&& !f.ApprovedDate_DT.HasValue
					&& !f.IsOpenDataRequest_FLAG;
	}

	public async Task<List<SharedDataFile>> GetProjectSharingRequestsAwaitingApproval(string projectCode)
	{
		return await _projectDbContext.SharedDataFiles
			.Where(GenerateSharingRequestsAwaitingApprovalCondition(projectCode))
			.ToListAsync();
	}

	public async Task<int> GetDataSharingRequestsAwaitingApprovalCount(string projectCode)
	{
		return await _projectDbContext.SharedDataFiles
			.CountAsync(GenerateSharingRequestsAwaitingApprovalCondition(projectCode));
	}

	public async Task<int> GetUsersOwnDataSharingRequestsCount(string projectCode, string requestingUserId)
	{
		return await _projectDbContext.SharedDataFiles
			.CountAsync(f => f.ProjectCode_CD != null
							 && f.ProjectCode_CD.ToLower() == projectCode.ToLower()
							 && f.RequestingUser_ID.ToLower() == requestingUserId.ToLower());
	}

	public async Task<SharedDataFile> LoadPublicUrlSharedFileInfo(Guid fileId)
	{
		return await _projectDbContext.SharedDataFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
	}

	public async Task<OpenDataSharedFile> LoadOpenDataSharedFileInfo(Guid fileId)
	{
		return await _projectDbContext.OpenDataSharedFiles.FirstOrDefaultAsync(e => e.File_ID == fileId);
	}

	public async Task<bool> MarkMetadataComplete(Guid fileId)
	{
		var submission = await LoadPublicUrlSharedFileInfo(fileId);
		if (submission != null)
		{
			submission.MetadataCompleted_FLAG = true;
			var result = await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
			if (result < 1)
			{
				_logger.LogError($"Error marking metadata complete for file {fileId}");
				return false;
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	public async Task<bool> SubmitPublicUrlShareForApproval(Guid fileId)
	{
		var submission = await LoadPublicUrlSharedFileInfo(fileId);
		if (submission != null)
		{
			submission.SubmittedDate_DT = DateTime.UtcNow;
			var result = await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
			if (result < 1)
			{
				_logger.LogError($"Error submitting file {fileId}");
				return false;
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	public async Task<bool> SaveTempSharingExpiryDate(Guid fileId, DateTime? expiryDate)
	{
		var submission = await LoadPublicUrlSharedFileInfo(fileId);
		if (submission != null)
		{
			submission.ExpirationDate_DT = expiryDate;
			var result = await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
			if (result < 1)
			{
				_logger.LogError($"Error saving expiration date for {fileId}");
				return false;
			}

			return true;
		}
		else
		{
			return false;
		}
	}

	public async Task ApprovePublicUrlShare(Guid fileId, DateTime? publicationDate = null)
	{
		var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
		if (shareInfo is not null)
		{
			shareInfo.ApprovedDate_DT = DateTime.UtcNow;
			shareInfo.PublicationDate_DT = publicationDate ?? shareInfo.ApprovedDate_DT;
			await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
		}
	}

	public async Task ApproveOpenDataShare(Guid fileId, DateTime publicationDate)
	{
		var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
		if (shareInfo is not null)
		{
			shareInfo.ApprovedDate_DT = DateTime.UtcNow;
			shareInfo.PublicationDate_DT = publicationDate;
			await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
		}
	}

	public async Task<OpenDataSharedFile> UpdateOpenDataPublication(Guid fileId, bool urlSharing)
	{
		var shareInfo = await LoadOpenDataSharedFileInfo(fileId);
		if (shareInfo is not null)
		{
			shareInfo.FileStorage_CD = urlSharing ? FileStorageType.Datahub : FileStorageType.OpenData;
			await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
		}
		return shareInfo;
	}

	public async Task DenyPublicUrlShare(Guid fileId) => await DoDeletePublicUrlShare(fileId);

	public async Task UpdateOpenDataApprovalFormId(Guid fileId, int approvalFormId)
	{
		var shareInfo = await LoadOpenDataSharedFileInfo(fileId);

		shareInfo.ApprovalForm_ID = approvalFormId;
		shareInfo.ApprovalFormEdited_FLAG = true;

		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	public async Task UpdateOpenDataSignedApprovalFormUrl(Guid fileId, string url)
	{
		var shareInfo = await LoadOpenDataSharedFileInfo(fileId);

		shareInfo.SignedApprovalForm_URL = url;
		shareInfo.SubmittedDate_DT = DateTime.UtcNow;

		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	public async Task<List<SharedDataFile>> GetAllSharedDataForProject(string projectCode)
	{
		return await _projectDbContext.SharedDataFiles
			.Where(f => f.ProjectCode_CD != null && f.ProjectCode_CD.ToLower() == projectCode.ToLower())
			.ToListAsync();
	}

	public async Task<bool> IsUserProjectDataApprover(string projectCode, string userId)
	{
		var projectUserEntries = await _projectDbContext.Project_Users
			.Where(p => p.Project.Project_Acronym_CD == projectCode && p.User_ID == userId)
			.ToListAsync();
		return projectUserEntries.Any(p => p.Role.IsAtLeastCollaborator);
	}

	public async Task<Datahub_Project> GetProjectWithUsers(string projectCode)
	{
		var project = await _projectDbContext.Projects
			.Where(p => p.Project_Acronym_CD == projectCode)
			.Include(p => p.Users)
			.SingleOrDefaultAsync();
		return project;
	}

	private async Task DoDeletePublicUrlShare(Guid fileId)
	{
		var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
		_projectDbContext.SharedDataFiles.Remove(shareInfo);
		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	private async Task DoDeleteOpenDataShare(Guid fileId)
	{
		var shareInfo = await LoadOpenDataSharedFileInfo(fileId);
		if (shareInfo.ApprovalForm_ID.HasValue)
		{
			await _metadataService.DeleteApprovalForm(shareInfo.ApprovalForm_ID.Value);
		}
		_projectDbContext.OpenDataSharedFiles.Remove(shareInfo);
		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	public async Task CancelPublicDataShare(Guid fileId)
	{
		var shareInfo = await LoadPublicUrlSharedFileInfo(fileId);
		if (shareInfo.IsOpenDataRequest_FLAG)
		{
			await DoDeleteOpenDataShare(fileId);
		}
		else
		{
			await DoDeletePublicUrlShare(fileId);
		}
	}

	public string GetOpenDataApprovalFormPdfUrl(int approvalFormId)
	{
		//https://app.powerbi.com/groups/6ca76417-b205-43c2-be1b-6aeeedcb84ae/rdlreports/04b8625f-f532-43fe-89d5-5911bbabd79b?rp:p_approval_form_id=9&rdl:format=pdf
		return $"{_config.OpenDataApprovalPdfBaseUrl}?{_config.OpenDataApprovalPdfFormIdParam}={approvalFormId}&rdl:format=pdf";
	}

	public string GetPublicSharedFileUrl(string fileId)
	{
		var ub = new UriBuilder(_config.PublicFileSharingDomain);
		ub.Path = $"/Public/DownloadFile/{fileId}";
		return ub.ToString();
	}

	public string GetPublicSharedUrl(string path)
	{
		var ub = new UriBuilder(_config.PublicFileSharingDomain) { Path = path };
		return ub.ToString();
	}

	public async Task<List<OpenDataSharedFile>> GetPendingApprovalOpenDataFiles()
	{
		return await _projectDbContext.OpenDataSharedFiles
			.Where(e => e.IsOpenDataRequest_FLAG && !e.ApprovedDate_DT.HasValue && !string.IsNullOrEmpty(e.SignedApprovalForm_URL))
			.ToListAsync();
	}

	public async Task SetPendingApprovalOpenDataAsRead(OpenDataSharedFile file)
	{
		file.ApprovalFormRead_FLAG = true;
		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	public async Task SetPendingApprovalOpenDataAsApproved(OpenDataSharedFile file)
	{
		file.ApprovedDate_DT = DateTime.UtcNow;
		await _projectDbContext.TrackSaveChangesAsync(_datahubAuditingService);
	}

	public async Task NotifySignedDocumentUploaded()
	{
		Dictionary<string, object> parameters = new()
		{
			{ "UserName", _config.OpenDataApproverName },
			{ "Url", _emailNotificationService.BuildAppLink("/share/dashboard") }
		};

		var emailBody = await _emailNotificationService.RenderTemplate<RequestForApprovalNotificationEmail>(parameters);
		var emailRecipients = new List<string>() { _config.OpenDataApproverEmail };

		await _emailNotificationService.SendEmailMessage(_config.OpenDataApproverEmailSubject, emailBody, emailRecipients, true);
	}

	public async Task NotifySignedDocumentApproved(string userName, string email, string requestTitle, string url)
	{
		Dictionary<string, object> parameters = new()
		{
			{ "UserName", userName },
			{ "Url", _emailNotificationService.BuildAppLink(url) },
			{ "RequestTitle", requestTitle }
		};
		var emailBody = await _emailNotificationService.RenderTemplate<ShareApprovedNotificationEmail>(parameters);
		var emailRecipients = new List<string>() { email };

		await _emailNotificationService.SendEmailMessage("Share approved / Partage approuvé", emailBody, emailRecipients, true);
	}
}