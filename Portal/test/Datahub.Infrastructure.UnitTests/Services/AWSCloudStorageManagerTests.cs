using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Datahub.Core.Data;
using Microsoft.AspNetCore.Components.Forms;
using Datahub.Infrastructure.Services.Storage;
using Datahub.Core.Storage;
using Moq;

namespace Datahub.Infrastructure.UnitTests.Services
{
    public class AWSCloudStorageManagerTests
    {
        private readonly AWSCloudStorageManager _manager = new("Display name", "key", "secret", "ca-central-1", "actual-bucket");

        [TestCase("old.csv", "new.csv")]
        [TestCase("folder/old.csv", "folder/new.csv")]
        [TestCase("folder/old.csv", "other/new.csv")]
        public async Task RenamePreservesObjectSettingsAndCopiesBeforeDeleting(string source, string destination)
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            var sequence = new MockSequence();
            client.InSequence(sequence).Setup(c => c.GetObjectMetadataAsync(
                    It.Is<GetObjectMetadataRequest>(r => r.BucketName == "actual-bucket" && r.Key == source), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetObjectMetadataResponse
                {
                    ContentLength = 123, ETag = "etag", VersionId = "version-1",
                    StorageClass = S3StorageClass.StandardInfrequentAccess,
                    ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                    ServerSideEncryptionKeyManagementServiceKeyId = "kms-key", BucketKeyEnabled = true
                });
            client.InSequence(sequence).Setup(c => c.CopyObjectAsync(It.Is<CopyObjectRequest>(r =>
                    r.SourceBucket == "actual-bucket" && r.DestinationBucket == "actual-bucket"
                    && r.SourceKey == source && r.DestinationKey == destination
                    && r.SourceVersionId == "version-1" && r.ETagToMatch == "etag"
                    && r.StorageClass == S3StorageClass.StandardInfrequentAccess
                    && r.MetadataDirective == S3MetadataDirective.COPY && r.TaggingDirective == TaggingDirective.COPY
                    && r.ServerSideEncryptionMethod == ServerSideEncryptionMethod.AWSKMS
                    && r.ServerSideEncryptionKeyManagementServiceKeyId == "kms-key" && r.BucketKeyEnabled == true), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyObjectResponse());
            client.InSequence(sequence).Setup(c => c.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r =>
                    r.BucketName == "actual-bucket" && r.Key == source && r.IfMatch == "etag" && r.VersionId == null), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteObjectResponse());

            Assert.That(await _manager.RenameFileAsync(client.Object, source, destination), Is.True);
            client.VerifyAll();
        }

        [Test]
        public async Task RenameToTheSameKeyDoesNotDeleteTheFile()
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            Assert.That(await _manager.RenameFileAsync(client.Object, "nested/file.txt", "nested/file.txt"), Is.True);
            client.VerifyNoOtherCalls();
        }

        [TestCase("", "new.csv")]
        [TestCase("old.csv", "")]
        public async Task RenameRejectsEmptyPaths(string source, string destination)
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            Assert.That(await _manager.RenameFileAsync(client.Object, source, destination), Is.False);
            client.VerifyNoOtherCalls();
        }

        [Test]
        public async Task FailedRenameCopyDoesNotDeleteTheSource()
        {
            var client = MetadataClient(new GetObjectMetadataResponse { ETag = "etag" });
            client.Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Copy failed"));
            Assert.That(await _manager.RenameFileAsync(client.Object, "nested/file.txt", "nested/renamed.txt"), Is.False);
            client.Verify(c => c.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task FailedRenameDeleteDoesNotRemoveTheCompletedCopy()
        {
            var client = MetadataClient(new GetObjectMetadataResponse { ETag = "etag" });
            client.Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyObjectResponse());
            client.Setup(c => c.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r => r.Key == "nested/file.txt"), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Delete failed"));
            Assert.That(await _manager.RenameFileAsync(client.Object, "nested/file.txt", "nested/renamed.txt"), Is.False);
            client.Verify(c => c.DeleteObjectAsync(It.Is<DeleteObjectRequest>(r => r.Key == "nested/renamed.txt"), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase("large")]
        [TestCase("archived")]
        [TestCase("encrypted")]
        [TestCase("unverified")]
        public async Task UnsupportedRenameSourcesAreNotCopiedOrDeleted(string reason)
        {
            var metadata = new GetObjectMetadataResponse { ETag = "etag" };
            if (reason == "large") metadata.ContentLength = 5L * 1024 * 1024 * 1024 + 1;
            if (reason == "archived") metadata.StorageClass = S3StorageClass.Glacier;
            if (reason == "encrypted") metadata.ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.AES256;
            if (reason == "unverified") metadata.ETag = null;
            var client = MetadataClient(metadata);
            Assert.That(await _manager.RenameFileAsync(client.Object, "nested/file.txt", "nested/renamed.txt"), Is.False);
            client.VerifyAll();
            client.Verify(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
            client.Verify(c => c.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [TestCase("", "filename.csv")]
        [TestCase("/", "filename.csv")]
        [TestCase("foldername", "foldername/filename.csv")]
        [TestCase("foldername/", "foldername/filename.csv")]
        [TestCase("/foldername/", "foldername/filename.csv")]
        [TestCase("foldername/nested", "foldername/nested/filename.csv")]
        [TestCase("foldername\\nested\\", "foldername/nested/filename.csv")]
        public async Task UploadUsesTheSelectedFolderAndConfiguredBucket(string folder, string expectedKey)
        {
            var stream = new MemoryStream(new byte[] { 1, 2, 3 });
            var browserFile = new Mock<IBrowserFile>(MockBehavior.Strict);
            browserFile.Setup(file => file.OpenReadStream(10L * 1024 * 1024 * 1024, It.IsAny<CancellationToken>()))
                .Returns(stream);
            var transfer = new Mock<ITransferUtility>(MockBehavior.Strict);
            transfer.Setup(utility => utility.UploadAsync(stream, "actual-bucket", expectedKey, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            var file = new PortalFileMetadata
            {
                id = "upload-file", filename = "filename.csv", folderpath = folder, BrowserFile = browserFile.Object
            };

            Assert.That(await _manager.UploadFileAsync(transfer.Object, file), Is.True);
            Assert.That(stream.CanRead, Is.False);
            transfer.VerifyAll();
            browserFile.VerifyAll();
        }

        [Test]
        public async Task UploadFailureReturnsFalseAndDisposesTheStream()
        {
            var stream = new MemoryStream();
            var browserFile = new Mock<IBrowserFile>();
            browserFile.Setup(file => file.OpenReadStream(It.IsAny<long>(), It.IsAny<CancellationToken>())).Returns(stream);
            var transfer = new Mock<ITransferUtility>();
            transfer.Setup(utility => utility.UploadAsync(stream, "actual-bucket", "foldername/filename.csv", It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Upload failed"));

            Assert.That(await _manager.UploadFileAsync(transfer.Object, new PortalFileMetadata
            {
                id = "upload-file", filename = "filename.csv", folderpath = "foldername", BrowserFile = browserFile.Object
            }), Is.False);
            Assert.That(stream.CanRead, Is.False);
        }

        [Test]
        public void StorageClassesIncludeInstantRetrievalAndExcludeLegacyDestinations()
        {
            Assert.That(_manager.GetFileStorageTiersList(), Is.EqualTo(new[]
            {
                "STANDARD", "STANDARD_IA", "ONEZONE_IA", "INTELLIGENT_TIERING", "GLACIER_IR", "GLACIER", "DEEP_ARCHIVE"
            }));
        }

        [TestCase(null, "STANDARD")]
        [TestCase("REDUCED_REDUNDANCY", "REDUCED_REDUNDANCY")]
        [TestCase("GLACIER_IR", "GLACIER_IR")]
        public async Task StorageClassLookupUsesMetadataAndDefaultsToStandard(string? storageClass, string expected)
        {
            var client = MetadataClient(new GetObjectMetadataResponse
            {
                StorageClass = storageClass == null ? null : S3StorageClass.FindValue(storageClass)
            });
            Assert.That(await _manager.GetFileStorageTierAsync(client.Object, "nested/file.txt"), Is.EqualTo(expected));
            client.VerifyAll();
        }

        [TestCase(null)]
        [TestCase("null")]
        [TestCase("version-1")]
        public async Task ClassChangeCopiesTheSameKeyAndPreservesMetadataTagsEncryptionAndSource(string? version)
        {
            var metadata = new GetObjectMetadataResponse
            {
                ContentLength = 5L * 1024 * 1024 * 1024,
                ETag = "etag",
                VersionId = version,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AWSKMS,
                ServerSideEncryptionKeyManagementServiceKeyId = "kms-key",
                BucketKeyEnabled = true
            };
            metadata.Metadata.Add("custom", "value");
            var client = MetadataClient(metadata);
            client.Setup(c => c.CopyObjectAsync(It.Is<CopyObjectRequest>(r =>
                    r.SourceBucket == "actual-bucket" && r.DestinationBucket == "actual-bucket"
                    && r.SourceKey == "nested/file.txt" && r.DestinationKey == "nested/file.txt"
                    && r.StorageClass == S3StorageClass.StandardInfrequentAccess
                    && r.MetadataDirective == S3MetadataDirective.COPY && r.TaggingDirective == TaggingDirective.COPY
                    && r.SourceVersionId == version && r.ETagToMatch == (version == null || version == "null" ? "etag" : null)
                    && r.ServerSideEncryptionMethod == ServerSideEncryptionMethod.AWSKMS
                    && r.ServerSideEncryptionKeyManagementServiceKeyId == "kms-key" && r.BucketKeyEnabled == true),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyObjectResponse());

            Assert.That(await _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD_IA"), Is.True);
            client.VerifyAll();
        }

        [TestCase("Hot")]
        [TestCase("REDUCED_REDUNDANCY")]
        [TestCase("standard")]
        public void InvalidDestinationDoesNotCallS3(string tier)
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            Assert.ThrowsAsync<StorageTierChangeException>(() => _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", tier));
            client.VerifyNoOtherCalls();
        }

        [Test]
        public async Task AnUnchangedClassDoesNotCopy()
        {
            var client = MetadataClient(new GetObjectMetadataResponse());
            Assert.That(await _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD"), Is.True);
            client.VerifyAll();
        }

        [Test]
        public void LargeObjectsAreRejectedBeforeCopying()
        {
            var client = MetadataClient(new GetObjectMetadataResponse { ContentLength = 5L * 1024 * 1024 * 1024 + 1 });
            var error = Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD_IA"));
            Assert.That(error!.Message, Does.Contain("5 GB"));
            client.VerifyAll();
        }

        [Test]
        public void CustomerProvidedEncryptionIsRejected()
        {
            var client = MetadataClient(new GetObjectMetadataResponse
            {
                ServerSideEncryptionCustomerMethod = ServerSideEncryptionCustomerMethod.AES256
            });
            Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD_IA"));
            client.VerifyAll();
        }

        [TestCase("STANDARD", null, false, 0, CloudStorageArchiveState.Available)]
        [TestCase("GLACIER_IR", null, false, 0, CloudStorageArchiveState.Available)]
        [TestCase("GLACIER", null, false, 0, CloudStorageArchiveState.RestoreRequired)]
        [TestCase("DEEP_ARCHIVE", null, true, 0, CloudStorageArchiveState.Restoring)]
        [TestCase("GLACIER", null, false, 1, CloudStorageArchiveState.Available)]
        [TestCase("GLACIER", null, false, -1, CloudStorageArchiveState.RestoreRequired)]
        [TestCase("INTELLIGENT_TIERING", "ARCHIVE_ACCESS", false, 0, CloudStorageArchiveState.RestoreRequired)]
        [TestCase("INTELLIGENT_TIERING", "DEEP_ARCHIVE_ACCESS", true, 0, CloudStorageArchiveState.Restoring)]
        [TestCase("INTELLIGENT_TIERING", null, false, 0, CloudStorageArchiveState.Available)]
        public async Task ArchiveAvailabilityReflectsMetadata(string tier, string? archive, bool restoring, int expiryDays, CloudStorageArchiveState expected)
        {
            var metadata = new GetObjectMetadataResponse
            {
                StorageClass = S3StorageClass.FindValue(tier),
                ArchiveStatus = archive == null ? null : ArchiveStatus.FindValue(archive),
                RestoreInProgress = restoring,
                RestoreExpiration = expiryDays == 0 ? null : DateTime.UtcNow.AddDays(expiryDays)
            };
            var client = MetadataClient(metadata);
            var status = await _manager.GetFileArchiveStatusAsync(client.Object, "nested/file.txt");
            Assert.That(status.State, Is.EqualTo(expected));
            Assert.That(status.RestoredCopyExpiry, Is.EqualTo(expected == CloudStorageArchiveState.Available ? metadata.RestoreExpiration : null));
        }

        [Test]
        public void AnUnavailableArchiveCannotBeCopied()
        {
            var client = MetadataClient(new GetObjectMetadataResponse { StorageClass = S3StorageClass.Glacier });
            Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD"));
        }

        [Test]
        public async Task ARestoredArchiveCanBeCopied()
        {
            var client = MetadataClient(new GetObjectMetadataResponse
            {
                StorageClass = S3StorageClass.Glacier, RestoreExpiration = DateTime.UtcNow.AddDays(1), ETag = "etag"
            });
            client.Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CopyObjectResponse());
            Assert.That(await _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD"), Is.True);
            client.VerifyAll();
        }

        [Test]
        public void CopyFailuresPropagate()
        {
            var client = MetadataClient(new GetObjectMetadataResponse { ETag = "etag" });
            client.Setup(c => c.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Access denied"));
            Assert.ThrowsAsync<AmazonS3Exception>(() => _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "GLACIER_IR"));
        }

        [Test]
        public void MissingSourceConditionsPreventCopying()
        {
            var client = MetadataClient(new GetObjectMetadataResponse());
            Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD_IA"));
        }

        [Test]
        public void MetadataFailuresPropagateWithoutCopying()
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            client.Setup(c => c.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Access denied"));
            Assert.ThrowsAsync<AmazonS3Exception>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "nested/file.txt", "STANDARD_IA"));
        }

        private static Mock<IAmazonS3> MetadataClient(GetObjectMetadataResponse response)
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            client.Setup(c => c.GetObjectMetadataAsync(
                    It.Is<GetObjectMetadataRequest>(r => r.BucketName == "actual-bucket" && r.Key == "nested/file.txt"),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
            return client;
        }

        [Test]
        public async Task ListFoldersAsyncCountsDirectFilesAndIncludesImplicitAndEmptyFoldersAcrossPages()
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            client.Setup(c => c.ListObjectsV2Async(
                    It.Is<ListObjectsV2Request>(r => r.BucketName == "actual-bucket" && r.Prefix == "" && r.ContinuationToken == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page(true, "next", "root.txt", "reports/one.txt", "reports/nested/two.txt", "empty/"));
            client.Setup(c => c.ListObjectsV2Async(
                    It.Is<ListObjectsV2Request>(r => r.ContinuationToken == "next"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page(false, null, "reports/three.txt", "Reports/four.txt", "zero-byte.txt"));

            var folders = await _manager.ListFoldersAsync(client.Object, "/");

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int>
            {
                [""] = 2,
                ["reports/"] = 2,
                ["reports/nested/"] = 1,
                ["empty/"] = 0,
                ["Reports/"] = 1
            }));
            client.VerifyAll();
        }

        [TestCase("reports")]
        [TestCase("reports/")]
        public async Task ListFoldersAsyncRestrictsListingToTheRequestedFolder(string prefix)
        {
            var client = new Mock<IAmazonS3>(MockBehavior.Strict);
            client.Setup(c => c.ListObjectsV2Async(
                    It.Is<ListObjectsV2Request>(r => r.BucketName == "actual-bucket" && r.Prefix == "reports/" && r.Delimiter == null),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Page(false, null, "reports/", "reports/file.txt", "reports/nested/deep/file.txt"));

            var folders = await _manager.ListFoldersAsync(client.Object, prefix);

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int>
            {
                ["reports/"] = 1,
                ["reports/nested/"] = 0,
                ["reports/nested/deep/"] = 1
            }));
            client.VerifyAll();
        }

        [TestCase("", "")]
        [TestCase("/", "")]
        [TestCase("empty", "empty/")]
        public async Task ListFoldersAsyncReturnsZeroForAnEmptyFolder(string prefix, string expectedFolder)
        {
            var client = new Mock<IAmazonS3>();
            client.Setup(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ListObjectsV2Response { IsTruncated = false });

            var folders = await _manager.ListFoldersAsync(client.Object, prefix);

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int> { [expectedFolder] = 0 }));
        }

        [Test]
        public void ListFoldersAsyncPropagatesStorageFailures()
        {
            var client = new Mock<IAmazonS3>();
            client.Setup(c => c.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Access denied"));

            Assert.ThrowsAsync<AmazonS3Exception>(() => _manager.ListFoldersAsync(client.Object, ""));
        }

        private static ListObjectsV2Response Page(bool truncated, string? token, params string[] keys)
        {
            return new ListObjectsV2Response
            {
                IsTruncated = truncated,
                NextContinuationToken = token,
                S3Objects = keys.Select(key => new S3Object { Key = key }).ToList()
            };
        }
    }
}
