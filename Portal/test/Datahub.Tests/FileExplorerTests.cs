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
using System.Linq;
using System.Collections.Generic;

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

        // make the mock available to individual tests so they can change IsLoggedInThroughEntra
        private readonly Mock<IUserInformationService> _mockUserInfo;

        // Seed value used when creating the in-memory workspace; tests can modify this before rendering
        private ExternalUserUploadLimit _externalUserUploadLimit = new(10L, 10);

        public FileExplorerTests()
        {
            _ctx = new BunitContext();

            // Mock minimal services used by the component
            _mockUserInfo = new Mock<IUserInformationService>();
            _mockUserInfo.Setup(x => x.GetCurrentUserEntraId()).ReturnsAsync(TestUserId);

            // ensure Heading can obtain the current portal user
            var portalUser = new PortalUser
            {
                Id =1,
                EntraUser = new EntraUser { GraphGuid = TestUserId, PortalUser = null! },
                Email = "test@domain.com",
                DisplayName = "Test User"
            };

            _mockUserInfo.Setup(x => x.GetCurrentPortalUserAsync()).ReturnsAsync(portalUser);
            _ctx.Services.AddSingleton<IUserInformationService>(_mockUserInfo.Object);

            // Mock IMSGraphService so ProfileCircle can fetch user display name if needed
            var mockGraphService = new Mock<IMSGraphService>();
            mockGraphService.SetupAllProperties();
            mockGraphService.Object.UsersDict = [];
            mockGraphService.Setup(g => g.GetUserAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((string id, CancellationToken ct) =>
                {
                    // create a lightweight MS Graph user
                    var mgUser = new Microsoft.Graph.Models.User { Id = id, DisplayName = portalUser.DisplayName, Mail = portalUser.Email };
                    var gu = GraphUser.Create(mgUser);

                    // cache in dictionary
                    mockGraphService.Object.UsersDict[id] = gu;
                    return gu;
                });

            _ctx.Services.AddSingleton<IMSGraphService>(mockGraphService.Object);

            var mockJs = new Mock<IJSRuntime>();
            mockJs
                .Setup(js => js.InvokeAsync<IJSObjectReference>(It.Is<string>(s => s == "import"), It.IsAny<object[]>()))
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
            // Use a stable in-memory database name for this test instance so multiple contexts share the same store
            var dbName = $"FileExplorerTestDb_{Guid.NewGuid():N}";
            var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
                .UseInMemoryDatabase(dbName).Options;

            DatahubProjectDBContext CreateAndSeedContext()
            {
                var ctx = new DatahubProjectDBContext(options);

                // ensure the workspace with the test acronym exists
                var existing = ctx.Projects.FirstOrDefault(p => p.Project_Acronym_CD == TestProjectAcronym);
                if (existing == null)
                {
                    ctx.Projects.Add(new Datahub_Project
                    {
                        Project_Acronym_CD = TestProjectAcronym,
                        MaxUploadMBForGccf = _externalUserUploadLimit.MaximumFileSizeMB,
                        MaxFileCountForGccf = _externalUserUploadLimit.MaximumFileCount
                    });
                    ctx.SaveChanges();
                }
                else
                {
                    // ensure max upload is set to the requested seed value for this context
                    existing.MaxUploadMBForGccf = _externalUserUploadLimit.MaximumFileSizeMB;
                    existing.MaxFileCountForGccf = _externalUserUploadLimit.MaximumFileCount;
                    ctx.SaveChanges();
                }

                return ctx;
            }

            var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            // Return a fresh context instance for each CreateDbContextAsync call to avoid disposed contexts across calls
            mockFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((CancellationToken ct) => CreateAndSeedContext());

            _ctx.Services.AddSingleton<IDbContextFactory<DatahubProjectDBContext>>(mockFactory.Object);

            // Provide IProjectUserManagementService mock so Heading can initialize
            var mockProjectUserMgmt = new Mock<IProjectUserManagementService>();
            mockProjectUserMgmt.Setup(m => m.GetProjectUsersAsync(It.IsAny<string>()))
                .ReturnsAsync(
                [
                    new UserRoleLinks
                    {
                        PortalUser = portalUser,
                        PortalUserId = portalUser.Id,
                        Role = new Project_Role { Id = (int)Project_Role.RoleNames.Collaborator, Name = "Collaborator", Description = "Collaborator" }
                    }
                ]);
            mockProjectUserMgmt.Setup(m => m.GetExternalUserUploadLimits(It.IsAny<string>()))
                .ReturnsAsync((string acronym) => _externalUserUploadLimit);

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
            public string Name { get; } = name;
            public DateTimeOffset LastModified { get; } = DateTimeOffset.UtcNow;
            public long Size { get; } = size;
            public string ContentType { get; } = "application/octet-stream";

            public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
            {
                try { stream.Position = 0; } catch { }
                return stream;
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

        private IRenderedComponent<FileExplorer> RenderFileExplorerWithMockStorage(
            out Mock<ICloudStorageManager> mockStorageManager,
            out List<FileMetaData> uploadedFiles,
            Action<Mock<ICloudStorageManager>, List<FileMetaData>> configure = null)
        {
            mockStorageManager = new Mock<ICloudStorageManager>();
            var localUploadedFiles = new List<FileMetaData>();

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
                .ReturnsAsync([]);

            // by default, wire UploadFileAsync to add to localUploadedFiles when called
            mockStorageManager.Setup(m => m.UploadFileAsync(It.IsAny<string>(), It.IsAny<FileMetaData>(), It.IsAny<Action<long>>()))
                .ReturnsAsync((string container, FileMetaData file, Action<long> progress) =>
                {
                    localUploadedFiles.Add(file);
                    return true;
                });

            configure?.Invoke(mockStorageManager, localUploadedFiles);

            var container = new CloudStorageContainer(mockStorageManager.Object, TestContainerName);

            var portalUser = new PortalUser { Email = "test@test.com", EntraUser = new EntraUser { PortalUser = null!, GraphGuid = Guid.NewGuid().ToString() } };

            var comp = _ctx.Render<FileExplorer>(parameters => parameters
                .Add(p => p.ProjectId, 1)
                .Add(p => p.Container, container)
                .AddCascadingValue(nameof(FileExplorer.ProjectAcronym), TestProjectAcronym)
                .AddCascadingValue(nameof(FileExplorer.PortalUser), portalUser)
            );

            uploadedFiles = localUploadedFiles;
            return comp;
        }

        private async Task WaitForUploadsAsync(List<FileMetaData> uploadedFiles, int expectedCount, int timeoutMs = 5000)
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

        private async Task WaitForNoUploadsAsync(List<FileMetaData> uploadedFiles, int timeoutMs = 500)
        {
            var sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                if (uploadedFiles.Count > 0)
                    throw new Exception($"Unexpected uploads occurred: {uploadedFiles.Count}");
                await Task.Delay(20);
            }
        }

        // Simple test file record used by MemberData
        public record TestFile(string Name, long Size, bool ShouldUpload);

        public static IEnumerable<object[]> UploadCases()
        {
            var oneMb = 1024 * 1024;

            // Trusted user cases
            yield return new object[] { "Trusted - allowed", true, new ExternalUserUploadLimit(10L, 10), new TestFile[] { new("allowed.txt", 100, true) } };
            yield return new object[] { "Trusted - blocked ext", true, new ExternalUserUploadLimit(10L, 10), new TestFile[] { new("dangerous.exe", 3, false) } };
            yield return new object[] { "Trusted - mixed", true, new ExternalUserUploadLimit(10L, 10), new TestFile[] { new("good1.txt", 1, true), new("bad.exe", 2, false), new("good2.md", 3, true) } };
            yield return new object[] { "Trusted - oversize allowed", true, new ExternalUserUploadLimit(1L, 10), new TestFile[] { new("large.txt", oneMb + 1, true) } };
            // Trusted user multiple-files case (trusted users ignore max file count)
            yield return new object[] { "Trusted - multiple allowed", true, new ExternalUserUploadLimit(10L, 1), new TestFile[] { new("file1.txt", 100, true), new("file2.md", 200, true) } };

            // External user cases (respect size and extension)
            yield return new object[] { "External - small allowed", false, new ExternalUserUploadLimit(1L, 10), new TestFile[] { new("small.txt", oneMb - 1, true) } };
            yield return new object[] { "External - large blocked", false, new ExternalUserUploadLimit(1L, 10), new TestFile[] { new("large.txt", oneMb + 1, false) } };
            yield return new object[] { "External - blocked ext", false, new ExternalUserUploadLimit(1L, 10), new TestFile[] { new("evil.exe", 10, false) } };
            yield return new object[] { "External - mixed", false, new ExternalUserUploadLimit(1L, 10), new TestFile[] { new("ok.txt", oneMb - 10, true), new("toolarge.txt", oneMb + 10, false), new("bad.exe", 5, false) } };

            // External user: batch size equal to limit -> allowed (subject to size/extension checks)
            yield return new object[] { "External - at max count", false, new ExternalUserUploadLimit(10L, 3), new TestFile[] { new("a.txt", 100, true), new("b.txt", 200, true), new("c.md", 300, true) } };

            // External user: batch size greater than allowed -> entire batch blocked (no files should upload)
            yield return new object[] { "External - too many files", false, new ExternalUserUploadLimit(10L, 2), new TestFile[] { new("one.txt", 100, false), new("two.txt", 200, false), new("three.md", 300, false) } };
        }

        [Theory]
        [MemberData(nameof(UploadCases))]
        public async Task UploadFiles_Parameterized(string caseName, bool isTrusted, ExternalUserUploadLimit externalUserUploadLimit, TestFile[] files)
        {
            // arrange per-case
            _mockUserInfo.Setup(x => x.IsLoggedInThroughEntra()).ReturnsAsync(isTrusted);
            _externalUserUploadLimit = externalUserUploadLimit;

            var comp = RenderFileExplorerWithMockStorage(out var mockStorageManager, out var uploadedFiles, (m, list) =>
            {
                // default to no existing file to allow uploads
                m.Setup(x => x.FileExistsAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);
                // let default UploadFileAsync behavior add to uploadedFiles (list)
            });

            var browserFiles = files.Select(f => CreateFakeBrowserFile(f.Name, new byte[(int)f.Size])).Cast<IBrowserFile>().ToArray();

            // act
            await InvokeDropZoneWithFilesAsync(comp, browserFiles);

            var expected = files.Where(f => f.ShouldUpload).Select(f => f.Name).OrderBy(n => n).ToArray();

            if (expected.Length > 0)
            {
                await WaitForUploadsAsync(uploadedFiles, expected.Length, 5000);
            }
            else
            {
                await WaitForNoUploadsAsync(uploadedFiles, 1000);
            }

            var actual = uploadedFiles.Select(f => f.filename).OrderBy(n => n).ToArray();
            Assert.Equal(expected, actual);
        }

    }
}
