using Microsoft.Graph;
using Microsoft.Kiota.Abstractions.Serialization;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Serialization.Json;
using Moq;
using System.Text;
using Microsoft.Kiota.Abstractions.Store;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker;
using NSubstitute;
using System.Net;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using Azure.Core.Serialization;
using Microsoft.Extensions.Options;
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using Datahub.Core.Model.Projects;
using Datahub.Shared;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Subscriptions;
using Datahub.Core.Data.Databricks;
using Datahub.Application.Services.WebApp;
using Microsoft.Graph.Invitations;
using Microsoft.Graph.Models;
using MediatR;

namespace Datahub.Functions.UnitTests
{
    public static class TestHelper
    {
        public const string TEST_PROJECT_ACRONYM = "TEST";
        public const string ACTIVE_WEB_APP_PROJECT_ACRONYM = "WAP";
        public const string INACTIVE_WEB_APP_PROJECT_ACRONYM = "IWAP";
        public const string OVERBUDGET_WEB_APP_PROJECT_ACRONYM = "OVER";
        public const string ACTIVE_WEB_APP_SERVICE_ID = "active-webapp";

        /// <summary>
        /// Mocking GraphServiceClient
        /// based on https://medium.com/@carlosedgarnovo_56347/unit-testing-microsoft-graphserviceclient-in-c-net-d86a33e9158b
        /// </summary>
        /// <returns>Mock GraphServiceClient</returns>
        public static GraphServiceClient MockGraphServiceClient()
        {
            Mock<IRequestAdapter> _requestAdapterMock = new();
            Mock<ISerializationWriterFactory> _serializationWriterFactoryMock = new();

            _serializationWriterFactoryMock.Setup(factory => factory.GetSerializationWriter(It.IsAny<string>())).Returns(new JsonSerializationWriter());

            _requestAdapterMock.SetupGet(adapter => adapter.BaseUrl).Returns("http://graph.test.internal/mock");
            _requestAdapterMock.SetupSet(adapter => adapter.BaseUrl = It.IsAny<string>());
            _requestAdapterMock.Setup(adapter => adapter.EnableBackingStore(It.IsAny<IBackingStoreFactory>()));
            _requestAdapterMock.SetupGet(adapter => adapter.SerializationWriterFactory).Returns(_serializationWriterFactoryMock.Object);

             // Mock SendAsync to return a fake Invitation when called by Invitations.PostAsync
             _requestAdapterMock.Setup(adapter => adapter.SendAsync<Invitation>(
                    It.IsAny<RequestInformation>(),
                    It.IsAny<ParsableFactory<Invitation>>(),
                    It.IsAny<Dictionary<string, ParsableFactory<IParsable>>>(),
                    It.IsAny<CancellationToken>()
            )).ReturnsAsync(new Invitation
            {
                Id = "mock-invitation-id",
                InviteRedeemUrl = "https://mocked-invite-link.com",
                Status = "Pending",
                InvitedUser = new User { Id= "mockUser" } 
                // "mockUser" is used in CreateGraphUser to skip hard-to-mock call like follows:
                // await graphClient.Groups[$"{groupId}"].Members.Ref.PostAsync(requestBody);
            });
            return new GraphServiceClient(_requestAdapterMock.Object);
        }

        public static HttpRequestData CreateHttpRequestData(string requestBody)
        {
            var context = Substitute.For<FunctionContext>();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(Options.Create(new WorkerOptions
            {
                Serializer = new JsonObjectSerializer()
            }));
            var serviceProvider = serviceCollection.BuildServiceProvider();

            context.InstanceServices.Returns(serviceProvider);

            return new FakeHttpRequestData(context, new Uri("http://localhost"), new MemoryStream(Encoding.UTF8.GetBytes(requestBody)));
        }


        public static IDbContextFactory<DatahubProjectDBContext> CreateMockDbContextFactory(CancellationToken cancellationToken = default)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SqlServerDatahubContext>().UseInMemoryDatabase(new Guid().ToString());
            // create a mock factory to return the db context when CreateDbContextAsync is called
            var context = new SqlServerDatahubContext(optionsBuilder.Options);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            var mockFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            mockFactory
                .Setup(f => f.CreateDbContextAsync(CancellationToken.None))
                .ReturnsAsync(() => new SqlServerDatahubContext(optionsBuilder.Options));

            return mockFactory.Object;
        }


        public static IWorkspaceWebAppManagementService CreateMockWebAppManagementService()
        {
            var mockWebAppService = new Mock<IWorkspaceWebAppManagementService>();
            mockWebAppService
                .Setup(w => w.GetState(It.IsAny<string>()))
                .ReturnsAsync((string s) => s == TestHelper.ACTIVE_WEB_APP_SERVICE_ID);

            return mockWebAppService.Object;
        }

        public static async Task SeedDatabase(IDbContextFactory<DatahubProjectDBContext> contextFactory)
        {
            await using var context = await contextFactory.CreateDbContextAsync();

            var users = new List<PortalUser>
            {
                new PortalUser
                {
                    Id=1,
                    GraphGuid=Guid.NewGuid().ToString()
                }
            };
            var portalUsers = new List<Datahub_Project_User>
            {
                new Datahub_Project_User
                {
                    ProjectUser_ID=1,
                    PortalUser=new PortalUser
                    {
                        Id=1,
                        GraphGuid=Guid.NewGuid().ToString()
                    }
                }
            };
            var projects = new List<Datahub_Project>
            {
                new Datahub_Project()
                {
                    Project_ID = 1,
                    Project_Acronym_CD = TEST_PROJECT_ACRONYM,
                    Project_Name = "Test Workspace",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Deleted_DT = null,
                    DatahubAzureSubscription = new DatahubAzureSubscription
                    {
                        SubscriptionId = "test-subscription-id", SubscriptionName="test", TenantId="tenant-id"
                    },
                    Credits = new Project_Credits{ Current=10}
                },
                new()
                {
                    Project_ID = 2,
                    Project_Acronym_CD = ACTIVE_WEB_APP_PROJECT_ACRONYM,
                    Project_Name = "WebApp Test Project",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Deleted_DT = null,
                    DatahubAzureSubscription = new DatahubAzureSubscription
                    {
                        SubscriptionId = "test-subscription-id", SubscriptionName="test", TenantId="tenant-i"
                    },
                    Credits = new Project_Credits{ Current=10},
                    Resources =
                    [
                        new Project_Resources2
                        {
                            ResourceType = "terraform:azure-app-service",
                            JsonContent = "{\n" +
                                $"  \"app_service_id\": \"{ACTIVE_WEB_APP_SERVICE_ID}\",\n" +
                                "  \"app_service_hostname\": \"example.azurewebsites.net\",\n" +
                                "  \"app_service_rg\": \"test_rg\"\n" +
                                "}",
                            Status = TerraformStatus.Completed
                        }
                    ]
                },
                new()
                {
                    Project_ID = 3,
                    Project_Acronym_CD = INACTIVE_WEB_APP_PROJECT_ACRONYM,
                    Project_Name = "Inactive WebApp Test Project",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Deleted_DT = null,
                    Resources =
                    [
                        new Project_Resources2
                        {
                            ResourceType = "terraform:azure-app-service",
                            JsonContent = "{\n" +
                                "  \"app_service_id\": \"inactive_webapp\",\n" +
                                "  \"app_service_hostname\": \"example.azurewebsites.net\",\n" +
                                "  \"app_service_rg\": \"test_rg\"\n" +
                                "}",
                            Status = TerraformStatus.Completed
                        }
                    ]
                }, new()
                {
                    Project_ID=4,
                    Project_Acronym_CD = OVERBUDGET_WEB_APP_PROJECT_ACRONYM,
                    Project_Name = "Overbudget WebApp Test Project",
                    Project_Status_Desc = "Active",
                    Sector_Name = "Test Sector",
                    Credits = new Project_Credits{ Current = 300},
                    Project_Budget = 200,
                    Users = portalUsers
                }
            };
            var resources = new List<Project_Resources2>
            {
                new Project_Resources2
                {
                    Project = new Datahub_Project
                    {
                        Project_ID=5,
                        Project_Acronym_CD=OVERBUDGET_WEB_APP_PROJECT_ACRONYM,
                    },
                    Status=TerraformStatus.Completed,
                    ResourceType = "terraform:azure-app-service"
                }
            };
            await context.Projects.AddRangeAsync(projects);
            await context.Project_Resources2.AddRangeAsync(resources);
            await context.SaveChangesAsync();
        }

        public class FakeHttpRequestData : HttpRequestData
        {
            public FakeHttpRequestData(FunctionContext functionContext, Uri url, Stream body = null) : base(functionContext)
            {
                Url = url;
                Body = body ?? new MemoryStream();
            }
            public override Stream Body { get; } = new MemoryStream();
            public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();
            public override IReadOnlyCollection<IHttpCookie> Cookies { get; }
            public override Uri Url { get; }
            public override IEnumerable<ClaimsIdentity> Identities { get; }
            public override string Method { get; }
            public override HttpResponseData CreateResponse()
            {
                return new FakeHttpResponseData(FunctionContext);
            }
        }
        public class FakeHttpResponseData : HttpResponseData
        {
            public FakeHttpResponseData(FunctionContext functionContext) : base(functionContext)
            {
            }

            public override HttpStatusCode StatusCode { get; set; }
            public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();
            public override Stream Body { get; set; } = new MemoryStream();
            public override HttpCookies Cookies { get; }

            public async Task WriteAsJsonAsync<T>(T content)
            {
                await Task.CompletedTask;
            }

        }
    }
}
