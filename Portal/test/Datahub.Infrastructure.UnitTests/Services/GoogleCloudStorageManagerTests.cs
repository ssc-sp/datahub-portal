using Datahub.Application.Services.Security;
using Datahub.Core.Data;
using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Google.Api.Gax;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using GObject = Google.Apis.Storage.v1.Data.Object;
using GObjects = Google.Apis.Storage.v1.Data.Objects;
using GBuckets = Google.Apis.Storage.v1.Data.Buckets;

namespace Datahub.Infrastructure.UnitTests.Services
{
    public class GoogleCloudStorageManagerTests
    {
        private readonly GoogleCloudStorageManager _manager = new(
            NullLoggerFactory.Instance, "project", "credentials", "display name");

        [Test]
        public async Task ConfiguredBucketIsValidatedWithoutListingProjectBuckets()
        {
            var manager = new GoogleCloudStorageManager(
                NullLoggerFactory.Instance, "project", "credentials", "display name", "  restricted-bucket  ");
            var objects = new Mock<PagedAsyncEnumerable<GObjects, GObject>>(MockBehavior.Strict);
            objects.Setup(page => page.ReadPageAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Page<GObject>)null!);
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(storage => storage.ListObjectsAsync(
                    "restricted-bucket", null,
                    It.Is<ListObjectsOptions>(options => options.PageSize == 1)))
                .Returns(objects.Object);

            Assert.That(await manager.GetContainersAsync(client.Object), Is.EqualTo(new[] { "restricted-bucket" }));
            client.VerifyAll();
            objects.VerifyAll();
            client.Verify(storage => storage.ListBucketsAsync(
                It.IsAny<string>(), It.IsAny<ListBucketsOptions>()), Times.Never);
        }

        [Test]
        public async Task BlankBucketRetainsProjectWideBucketDiscovery()
        {
            var manager = new GoogleCloudStorageManager(
                NullLoggerFactory.Instance, string.Empty, "{\"project_id\":\"credential-project\"}", "display name", "  ");
            var buckets = new Mock<PagedAsyncEnumerable<GBuckets, Bucket>>(MockBehavior.Strict);
            buckets.Setup(page => page.ReadPageAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Page<Bucket>([new Bucket { Id = "first" }, new Bucket { Id = "second" }], null));
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(storage => storage.ListBucketsAsync(
                    "credential-project", It.Is<ListBucketsOptions>(options => options.PageSize == 100)))
                .Returns(buckets.Object);

            Assert.That(await manager.GetContainersAsync(client.Object), Is.EqualTo(new[] { "first", "second" }));
            client.VerifyAll();
            buckets.VerifyAll();
            client.Verify(storage => storage.ListObjectsAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<ListObjectsOptions>()), Times.Never);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task StorageMetadataReportsBucketAutoclass(bool enabled)
        {
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(storage => storage.GetBucketAsync(
                    "bucket", It.IsAny<GetBucketOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Bucket
                {
                    Name = "bucket",
                    Autoclass = new Bucket.AutoclassData { Enabled = enabled }
                });

            var metadata = await _manager.GetStorageMetadataAsync(client.Object, "bucket");

            Assert.That(metadata, Is.TypeOf<GoogleCloudStorageMetadata>());
            Assert.That(((GoogleCloudStorageMetadata)metadata).AutoclassEnabled, Is.EqualTo(enabled));
            client.VerifyAll();
        }

        [Test]
        public void NewConnectionDataIncludesGcpProjectAndOptionalBucket()
        {
            var connectionData = CloudStorageManagerFactory.CreateNewStorageProperties();

            Assert.That(connectionData[CloudStorageHelpers.GCP_ProjectId], Is.Empty);
            Assert.That(connectionData[CloudStorageHelpers.GCP_BucketName], Is.Empty);
            Assert.That(CloudStorageHelpers.All_Keys, Does.Contain(CloudStorageHelpers.GCP_ProjectId));
            Assert.That(CloudStorageHelpers.All_Keys, Does.Contain(CloudStorageHelpers.GCP_BucketName));
        }

        [Test]
        public void LegacyConnectionDataWithoutNewKeysCanStillCreateAManager()
        {
            var factory = new CloudStorageManagerFactory(
                NullLoggerFactory.Instance, Mock.Of<IKeyVaultUserService>());
            var connectionData = new Dictionary<string, string>
            {
                [CloudStorageHelpers.GCP_Json] = "{\"project_id\":\"legacy-project\"}"
            };

            var manager = factory.CreateTestCloudStorageManager(CloudStorageProviderType.GCP, connectionData);

            Assert.That(manager, Is.TypeOf<GoogleCloudStorageManager>());
        }

        [TestCase("configured-project", "{\"project_id\":\"credential-project\"}", "configured-project")]
        [TestCase("", "{\"project_id\":\"credential-project\"}", "credential-project")]
        [TestCase("", "invalid JSON", "")]
        public void ProjectIdFallsBackToCredentialsForLegacyConnections(
            string configuredProjectId, string credentials, string expected)
        {
            Assert.That(GoogleCloudStorageManager.ResolveProjectId(configuredProjectId, credentials), Is.EqualTo(expected));
        }

        [Test]
        public async Task ListFoldersCountsDirectFilesAndIncludesImplicitAndEmptyFoldersAcrossPages()
        {
            var firstPage = ObjectPage("next", "root.txt", "reports/one.txt", "reports/nested/two.txt", "empty/");
            var secondPage = ObjectPage(null, "reports/three.txt", "Reports/four.txt", "zero-byte.txt");
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(storage => storage.ListObjectsAsync(
                    "bucket", "", It.Is<ListObjectsOptions>(options => options.PageSize == 100 && options.PageToken == null)))
                .Returns(firstPage.Object);
            client.Setup(storage => storage.ListObjectsAsync(
                    "bucket", "", It.Is<ListObjectsOptions>(options => options.PageSize == 100 && options.PageToken == "next")))
                .Returns(secondPage.Object);

            var folders = await _manager.ListFoldersAsync(client.Object, "bucket", "/");

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int>
            {
                [""] = 2,
                ["reports/"] = 2,
                ["reports/nested/"] = 1,
                ["empty/"] = 0,
                ["Reports/"] = 1
            }));
            client.VerifyAll();
            firstPage.VerifyAll();
            secondPage.VerifyAll();
        }

        [TestCase("reports")]
        [TestCase("reports/")]
        public async Task ListFoldersRestrictsListingToTheRequestedFolder(string prefix)
        {
            var page = ObjectPage(null, "reports/", "reports/file.txt", "reports/nested/deep/file.txt");
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(storage => storage.ListObjectsAsync(
                    "bucket", "reports/", It.Is<ListObjectsOptions>(options => options.PageSize == 100)))
                .Returns(page.Object);

            var folders = await _manager.ListFoldersAsync(client.Object, "bucket", prefix);

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int>
            {
                ["reports/"] = 1,
                ["reports/nested/"] = 0,
                ["reports/nested/deep/"] = 1
            }));
            client.VerifyAll();
            page.VerifyAll();
        }

        [TestCase("", "")]
        [TestCase("/", "")]
        [TestCase("empty", "empty/")]
        public async Task ListFoldersReturnsZeroForAnEmptyFolder(string prefix, string expectedFolder)
        {
            var page = ObjectPage(null);
            var client = new Mock<StorageClient>();
            client.Setup(storage => storage.ListObjectsAsync(
                    "bucket", expectedFolder, It.IsAny<ListObjectsOptions>()))
                .Returns(page.Object);

            var folders = await _manager.ListFoldersAsync(client.Object, "bucket", prefix);

            Assert.That(folders, Is.EquivalentTo(new Dictionary<string, int> { [expectedFolder] = 0 }));
        }

        [Test]
        public void ListFoldersPropagatesStorageFailures()
        {
            var client = new Mock<StorageClient>();
            client.Setup(storage => storage.ListObjectsAsync(
                    "bucket", "", It.IsAny<ListObjectsOptions>()))
                .Throws(new InvalidOperationException("Access denied"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _manager.ListFoldersAsync(client.Object, "bucket", ""));
        }

        [Test]
        public void StorageClassesIncludeOnlyModernDestinations()
        {
            Assert.That(_manager.GetFileStorageTiersList(), Is.EqualTo(new[]
            {
                "STANDARD", "NEARLINE", "COLDLINE", "ARCHIVE"
            }));
        }

        [TestCase("STANDARD", "Standard")]
        [TestCase("NEARLINE", "Nearline")]
        [TestCase("COLDLINE", "Coldline")]
        [TestCase("ARCHIVE", "Archive")]
        public void StorageClassesHaveReadableLabels(string storageClass, string label)
        {
            Assert.That(CloudStorageHelpers.GetStorageClassLabel(storageClass), Is.EqualTo(label));
        }

        [TestCase(null, "STANDARD")]
        [TestCase("NEARLINE", "NEARLINE")]
        [TestCase("REGIONAL", "REGIONAL")]
        public async Task StorageClassLookupUsesObjectMetadataAndDefaultsToStandard(string? storageClass, string expected)
        {
            var client = ObjectClient(new GObject { StorageClass = storageClass });

            Assert.That(await _manager.GetFileStorageTierAsync(client.Object, "bucket", "nested/file.txt"), Is.EqualTo(expected));
            client.VerifyAll();
        }

        [TestCase("standard")]
        [TestCase("REGIONAL")]
        [TestCase("MULTI_REGIONAL")]
        [TestCase("DURABLE_REDUCED_AVAILABILITY")]
        public void InvalidDestinationDoesNotCallGoogleCloud(string storageClass)
        {
            var client = new Mock<StorageClient>(MockBehavior.Strict);

            Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "bucket", "nested/file.txt", storageClass));
            client.VerifyNoOtherCalls();
        }

        [Test]
        public async Task AnUnchangedClassDoesNotRewriteTheObject()
        {
            var client = ObjectClient(new GObject { StorageClass = "COLDLINE", Generation = 12 });

            Assert.That(await _manager.SetFileStorageTierAsync(
                client.Object, "bucket", "nested/file.txt", "COLDLINE"), Is.True);
            client.VerifyAll();
            client.Verify(c => c.CopyObjectAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CopyObjectOptions>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task ClassChangeRewritesTheSameObjectWithGenerationGuardsAndKmsKey()
        {
            var source = new GObject
            {
                StorageClass = "STANDARD",
                Generation = 42,
                KmsKeyName = "projects/project/locations/global/keyRings/ring/cryptoKeys/key"
            };
            var client = ObjectClient(source);
            client.Setup(c => c.CopyObjectAsync(
                    "bucket", "nested/file.txt", "bucket", "nested/file.txt",
                    It.Is<CopyObjectOptions>(options =>
                        options.ExtraMetadata.StorageClass == "ARCHIVE"
                        && options.IfSourceGenerationMatch == 42
                        && options.IfGenerationMatch == 42
                        && options.KmsKeyName == source.KmsKeyName),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GObject { StorageClass = "ARCHIVE" });

            Assert.That(await _manager.SetFileStorageTierAsync(
                client.Object, "bucket", "nested/file.txt", "ARCHIVE"), Is.True);
            client.VerifyAll();
        }

        [Test]
        public void ClassChangeFailsWhenGoogleCloudIgnoresTheRequestedClass()
        {
            var client = ObjectClient(new GObject { StorageClass = "STANDARD", Generation = 42 });
            client.Setup(c => c.CopyObjectAsync(
                    "bucket", "nested/file.txt", "bucket", "nested/file.txt",
                    It.IsAny<CopyObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GObject { StorageClass = "STANDARD" });

            var exception = Assert.ThrowsAsync<StorageTierChangeException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "bucket", "nested/file.txt", "ARCHIVE"));

            Assert.That(exception!.Message, Does.Contain("Autoclass"));
            client.VerifyAll();
        }

        [Test]
        public void LookupFailuresPropagateWithoutRewriting()
        {
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(c => c.GetObjectAsync(
                    "bucket", "nested/file.txt", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Access denied"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "bucket", "nested/file.txt", "NEARLINE"));
            client.VerifyAll();
        }

        [Test]
        public void RewriteFailuresPropagate()
        {
            var client = ObjectClient(new GObject { StorageClass = "STANDARD", Generation = 42 });
            client.Setup(c => c.CopyObjectAsync(
                    It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
                    It.IsAny<CopyObjectOptions>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Rewrite failed"));

            Assert.ThrowsAsync<InvalidOperationException>(() =>
                _manager.SetFileStorageTierAsync(client.Object, "bucket", "nested/file.txt", "NEARLINE"));
            client.VerifyAll();
        }

        private static Mock<StorageClient> ObjectClient(GObject obj)
        {
            var client = new Mock<StorageClient>(MockBehavior.Strict);
            client.Setup(c => c.GetObjectAsync(
                    "bucket", "nested/file.txt", It.IsAny<GetObjectOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(obj);
            return client;
        }

        private static Mock<PagedAsyncEnumerable<GObjects, GObject>> ObjectPage(string? nextPageToken, params string[] names)
        {
            var page = new Mock<PagedAsyncEnumerable<GObjects, GObject>>(MockBehavior.Strict);
            page.Setup(objects => objects.ReadPageAsync(100, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Page<GObject>(names.Select(name => new GObject { Name = name }), nextPageToken));
            return page;
        }
    }
}
