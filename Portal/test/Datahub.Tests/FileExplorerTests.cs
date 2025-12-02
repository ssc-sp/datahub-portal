using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using Microsoft.JSInterop;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MudBlazor;
using System.Diagnostics;

using Datahub.Portal.Pages.Workspace.Storage;
using Datahub.Application.Configuration;
using Datahub.Core.Storage;
using Datahub.Core.Data;
using Datahub.Application.Services.UserManagement;
using Datahub.Application.Services.Achievements;
using Datahub.Application.Services.Publishing;
using Datahub.Infrastructure.Offline; // <-- add localization extension
using Microsoft.AspNetCore.Components;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Projects;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Core.Model.Context;
using Datahub.Core.Components.FileUpload;
using Datahub.Core.Model.Users;

namespace Datahub.Tests
{
    public class FileExplorerTests : IDisposable
    {
        private readonly Bunit.BunitContext _ctx;
        private const string TestUserId = "user-id";
        private const string TestProjectAcronym = "TEST";
        private const string TestContainerName = "test";

        public FileExplorerTests()
        {
            _ctx = new Bunit.BunitContext();

            // Mock minimal services used by the component
            var mockUserInfo = new Mock<IUserInformationService>();
            mockUserInfo.Setup(x => x.GetCurrentUserEntraId()).ReturnsAsync(TestUserId);

            // ensure Heading can obtain the current portal user
            var portalUser = new PortalUser
            {
                Id =1,
                EntraUser = new EntraUser { GraphGuid = TestUserId, PortalUser = null! },
                Email = "test@domain.com",
                DisplayName = "Test User"
            };

            mockUserInfo.Setup(x => x.GetCurrentPortalUserAsync()).ReturnsAsync(portalUser);
            _ctx.Services.AddSingleton<IUserInformationService>(mockUserInfo.Object);

            // Mock IMSGraphService so ProfileCircle can fetch user display name if needed
            var mockGraphService = new Mock<IMSGraphService>();
            mockGraphService.SetupAllProperties();
            mockGraphService.Object.UsersDict = new System.Collections.Generic.Dictionary<string, Datahub.Core.Data.GraphUser>();
            mockGraphService.Setup(g => g.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string id, CancellationToken ct) =>
                {
                    // create a lightweight MS Graph user
                    var mgUser = new Microsoft.Graph.Models.User { Id = id, DisplayName = portalUser.DisplayName, Mail = portalUser.Email };
                    var gu = Datahub.Core.Data.GraphUser.Create(mgUser);

                    // cache in dictionary
                    mockGraphService.Object.UsersDict[id] = gu;
                    return gu;
                });

            _ctx.Services.AddSingleton<IMSGraphService>(mockGraphService.Object);

            var mockJs = new Mock<IJSRuntime>();
            mockJs.Setup(js => js.InvokeAsync<IJSObjectReference>(It.Is<string>(s => s == "import"), It.IsAny<object[]>() ))
                .Returns(new ValueTask<IJSObjectReference>(Mock.Of<IJSObjectReference>()));
            _ctx.Services.AddSingleton<IJSRuntime>(mockJs.Object);

            _ctx.Services.AddSingleton<IPortalUserTelemetryService>(new Mock<IPortalUserTelemetryService>().Object);
            _ctx.Services.AddSingleton<IDialogService>(new Mock<IDialogService>().Object);
            _ctx.Services.AddSingleton<IOpenDataPublishingService>(new Mock<IOpenDataPublishingService>().Object);
            _ctx.Services.AddSingleton<ILogger<FileExplorer>>(new Mock<ILogger<FileExplorer>>().Object);

            // Provide a DatahubPortalConfiguration with default blocked extensions and register localization
            var config = new DatahubPortalConfiguration();
            _ctx.Services.AddSingleton(config);
            _ctx.Services.AddDatahubLocalization(config);

            // register an in-memory DbContextFactory for components that need a DB context
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
            var realCtx = new DatahubProjectDBContext(options);
            var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>())).ReturnsAsync(realCtx);
            _ctx.Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(mockFactory.Object);

            // Provide IProjectUserManagementService mock so Heading can initialize
            var mockProjectUserMgmt = new Mock<IProjectUserManagementService>();
            mockProjectUserMgmt.Setup(m => m.GetProjectUsersAsync(It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.List<UserRoleLinks>
                {
                    new UserRoleLinks
                    {
                        PortalUser = portalUser,
                        PortalUserId = portalUser.Id,
                        Role = new Project_Role { Id = (int)Project_Role.RoleNames.Collaborator, Name = "Collaborator", Description = "Collaborator" }
                    }
                });

            _ctx.Services.AddSingleton<IProjectUserManagementService>(mockProjectUserMgmt.Object);

            // Minimal stubs for other injected services used by Heading
            _ctx.Services.AddSingleton<IServiceAuthManager>(new Mock<IServiceAuthManager>().Object);
            _ctx.Services.AddSingleton<IMetadataBrokerService>(new Mock<IMetadataBrokerService>().Object);

        }

        public void Dispose()
        {
            _ctx.Dispose();
        }

        private class FakeBrowserFile(string name, long size, Stream stream) : IBrowserFile
        {
            private readonly Stream _stream = stream;
            public string Name { get; } = name;
            public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;
            public long Size { get; } = size;
            public string ContentType { get; } = "application/octet-stream";

            public Stream OpenReadStream(long maxAllowedSize =512000, CancellationToken cancellationToken = default)
            {
                // ensure stream position is at beginning
                try { _stream.Position =0; } catch { }
                return _stream;
            }
        }

        // Helper to create a FakeBrowserFile from a byte array. The returned FakeBrowserFile owns
        // the underlying MemoryStream, which will remain valid until the test completes.
        private static FakeBrowserFile CreateFakeBrowserFile(string name, byte[] contents)
        {
            var ms = new MemoryStream(contents ?? []);
            return new FakeBrowserFile(name, ms.Length, ms);
        }

        // Helper to invoke the DropZone's OnFilesDrop with the provided files for a rendered FileExplorer component
        private static async Task InvokeDropZoneWithFilesAsync(IRenderedComponent<FileExplorer> comp, params IBrowserFile[] files)
        {
            var args = new InputFileChangeEventArgs(new System.Collections.ObjectModel.ReadOnlyCollection<IBrowserFile>(files));
            var dropZone = comp.FindComponent<DropZone>();
            await dropZone.InvokeAsync(() => dropZone.Instance.OnFilesDrop.InvokeAsync(args));
        }

        private IRenderedComponent<FileExplorer> RenderFileExplorerWithMockStorage(out Mock<ICloudStorageManager> mockStorageManager,
            out System.Collections.Generic.List<FileMetaData> uploadedFiles,
            Action<Mock<ICloudStorageManager>, System.Collections.Generic.List<FileMetaData>> configure = null)
        {
            mockStorageManager = new Mock<ICloudStorageManager>();
            var localUploadedFiles = new System.Collections.Generic.List<FileMetaData>();

            // default behaviors used by the file explorer
            mockStorageManager.Setup(m => m.GetDfsPagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
                .ReturnsAsync(new DfsPage(["/folder1/"],
                    [
                        new FileMetaData { filename = "existing.txt", filesize = "123", name = "existing.txt", id = "12345" }
                    ],
                    null));

            mockStorageManager.Setup(m => m.GetStorageMetadataAsync(It.IsAny<string>()))
                .ReturnsAsync(new AzureStorageMetadata { Container = "test", Url = "https://example.com", GeoRedundancy = "test", StorageAccountType = "test", Versioning = "test" });

            mockStorageManager.Setup(m => m.ListFoldersAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new System.Collections.Generic.Dictionary<string, int>());

            // by default, wire UploadFileAsync to add to localUploadedFiles when called
            mockStorageManager.Setup(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<FileMetaData>(), It.IsAny<Action<long>>()))
                .ReturnsAsync((string container, FileMetaData file, Action<long> progress) =>
                {
                    localUploadedFiles.Add(file);
                    return true;
                });

            configure?.Invoke(mockStorageManager, localUploadedFiles);

            var container = new CloudStorageContainer(mockStorageManager.Object, TestContainerName);

            var graphUser = new Microsoft.Graph.Models.User { Mail = "test@domain.com" };

            var comp = _ctx.Render<FileExplorer>(parameters => parameters
                .Add(p => p.ProjectId,1)
                .Add(p => p.Container, container)
                .AddCascadingValue(nameof(FileExplorer.ProjectAcronym), TestProjectAcronym)
                .AddCascadingValue(nameof(FileExplorer.GraphUser), graphUser)
            );

            uploadedFiles = localUploadedFiles;
            return comp;
        }

        private async Task WaitForUploadsAsync(System.Collections.Generic.List<FileMetaData> uploadedFiles, int expectedCount, int timeoutMs =5000)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (uploadedFiles.Count >= expectedCount)
                    return;
                await Task.Delay(20);
            }
            throw new TimeoutException($"Timed out waiting for {expectedCount} uploads (got {uploadedFiles.Count})");
        }

        private async Task WaitForNoUploadsAsync(System.Collections.Generic.List<FileMetaData> uploadedFiles, int timeoutMs =500)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (uploadedFiles.Count >0)
                    throw new Exception($"Unexpected uploads occurred: {uploadedFiles.Count}");
                await Task.Delay(20);
            }
        }

        [Fact]
        public async Task UploadFile_BlockedExtension_AddsToBlockedFiles()
        {
            // Arrange and render component with mock storage
            var comp = RenderFileExplorerWithMockStorage(out var mockStorageManager, out var uploadedFiles);

            // create a browser file with blocked extension (.exe is blocked by default config)
            var blockedFile = CreateFakeBrowserFile("dangerous.exe", [1, 2, 3]);

            // Act - invoke DropZone with the blocked file
            await InvokeDropZoneWithFilesAsync(comp, blockedFile);

            // Wait a short time to ensure any upload attempts complete (deterministic)
            await WaitForNoUploadsAsync(uploadedFiles,1000);

            // Assert - blocked files should not be uploaded; ensure uploadedFiles does not contain the blocked filename
            Assert.DoesNotContain(uploadedFiles, f => f.filename == blockedFile.Name);
        }

        [Fact]
        public async Task UploadFile_AllowedExtension_CallsStorageUpload()
        {
            // Arrange and render component with mock storage
            var comp = RenderFileExplorerWithMockStorage(out var mockStorageManager, out var uploadedFiles, (m, list) =>
            {
                // File does not exist for overwrite check
                m.Setup(x => x.FileExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
                // let default UploadFileAsync behavior add to uploadedFiles (list)
            });

            var allowedFile = CreateFakeBrowserFile("allowed.txt", [4, 5, 6]);

            // Act - invoke DropZone with the allowed file
            await InvokeDropZoneWithFilesAsync(comp, allowedFile);

            // Wait for the upload to complete
            await WaitForUploadsAsync(uploadedFiles,1,3000);

            // Assert - UploadFileAsync should have been called and uploadedFiles should contain the file
            mockStorageManager.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<FileMetaData>(), It.IsAny<Action<long>>()), Times.AtLeastOnce);
            Assert.Contains(uploadedFiles, f => f.filename == allowedFile.Name);
        }

        [Fact]
        public async Task UploadMultipleFiles_MixedAllowedAndBlocked_ProcessesCorrectly()
        {
            // Arrange and render component with mock storage
            var comp = RenderFileExplorerWithMockStorage(out var mockStorageManager, out var uploadedFiles, (m, list) =>
            {
                // File does not exist for overwrite check
                m.Setup(x => x.FileExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
            });

            // create multiple browser files: two allowed and one blocked
            var allowed1 = CreateFakeBrowserFile("good1.txt", [1]);
            var blocked = CreateFakeBrowserFile("bad.exe", [2]);
            var allowed2 = CreateFakeBrowserFile("good2.md", [3]);

            // Act - invoke DropZone with multiple files
            await InvokeDropZoneWithFilesAsync(comp, allowed1, blocked, allowed2);

            // Wait for the two uploads to complete
            await WaitForUploadsAsync(uploadedFiles,2,5000);

            // Assert - only the two allowed files were uploaded
            mockStorageManager.Verify(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<FileMetaData>(), It.IsAny<Action<long>>()), Times.Exactly(2));
            Assert.Contains(uploadedFiles, f => f.filename == allowed1.Name);
            Assert.Contains(uploadedFiles, f => f.filename == allowed2.Name);
            Assert.DoesNotContain(uploadedFiles, f => f.filename == blocked.Name);
        }
    }
}
