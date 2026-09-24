using Datahub.Core.Storage;
using Datahub.Infrastructure.Services.Storage;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using GObject = Google.Apis.Storage.v1.Data.Object;

namespace Datahub.Infrastructure.UnitTests.Services
{
    public class GoogleCloudStorageManagerTests
    {
        private readonly GoogleCloudStorageManager _manager = new(
            NullLoggerFactory.Instance, "project", "credentials", "display name");

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
    }
}
