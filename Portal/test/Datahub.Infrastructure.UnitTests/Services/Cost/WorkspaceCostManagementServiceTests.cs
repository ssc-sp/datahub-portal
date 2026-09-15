using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.CostManagement;
using Azure.ResourceManager.CostManagement.Models;
using Datahub.Application.Services.Cost;
using Datahub.Application.Services.ResourceGroups;
using Datahub.Core.Model.Context;
using Datahub.Infrastructure.Services.Cost;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NSubstitute;

namespace Datahub.Infrastructure.UnitTests.Services.Cost;

[TestFixture]
public class WorkspaceCostManagementServiceTests
{
    private static readonly ResourceIdentifier Scope = new("/subscriptions/00000000-0000-0000-0000-000000000000");

    [Test]
    public async Task QueryScopeCostsAsync_ShouldSplitDateRange_WhenResponseIsPaginated()
    {
        // Arrange
        var sut = CreateService();
        sut.SetupSequence(service => service.ExecuteUsageQueryAsync(It.IsAny<ResourceIdentifier>(),
                It.IsAny<QueryDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QueryResponse(1, "https://management.azure.com/next"))
            .ReturnsAsync(QueryResponse(100))
            .ReturnsAsync(QueryResponse(100));

        // Act
        var result = await sut.Object.QueryScopeCostsAsync(Scope.ToString(), new DateTime(2026, 9, 1),
            new DateTime(2026, 9, 4), QueryGranularity.Total, ["rg-test"]);

        // Assert
        result.Should().HaveCount(2);
        result.Sum(cost => cost.Amount).Should().Be(200);
        sut.Verify(service => service.ExecuteUsageQueryAsync(It.IsAny<ResourceIdentifier>(),
            It.IsAny<QueryDefinition>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
        var ranges = sut.Invocations
            .Where(invocation => invocation.Method.Name == nameof(WorkspaceCostManagementService.ExecuteUsageQueryAsync))
            .Select(invocation => (QueryDefinition)invocation.Arguments[1])
            .Select(query => (query.TimePeriod.From.Date, query.TimePeriod.To.Date))
            .ToList();
        ranges.Should().Contain((new DateTime(2026, 9, 1), new DateTime(2026, 9, 2)));
        ranges.Should().Contain((new DateTime(2026, 9, 3), new DateTime(2026, 9, 4)));
    }

    [Test]
    public async Task QueryScopeCostsAsync_ShouldFailAfterOneRequest_WhenSingleDayIsPaginated()
    {
        // Arrange
        var sut = CreateService();
        sut.Setup(service => service.ExecuteUsageQueryAsync(It.IsAny<ResourceIdentifier>(),
                It.IsAny<QueryDefinition>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QueryResponse(1, "https://management.azure.com/next"));
        var date = new DateTime(2026, 9, 1);

        // Act
        var action = () => sut.Object.QueryScopeCostsAsync(Scope.ToString(), date, date, QueryGranularity.Daily,
            ["rg-test"]);

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*single-day range*");
        sut.Verify(service => service.ExecuteUsageQueryAsync(It.IsAny<ResourceIdentifier>(),
            It.IsAny<QueryDefinition>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Mock<WorkspaceCostManagementService> CreateService()
    {
        var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var dbContextFactory = Substitute.For<IDbContextFactory<DatahubProjectDBContext>>();
        dbContextFactory.CreateDbContextAsync(Arg.Any<CancellationToken>())
            .Returns(_ => new DatahubProjectDBContext(options));

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CostManagementQueryIntervalSeconds"] = "0"
            })
            .Build();

        return new Mock<WorkspaceCostManagementService>(Substitute.For<ArmClient>(),
            Substitute.For<ILogger<WorkspaceCostManagementService>>(), dbContextFactory,
            Substitute.For<IWorkspaceResourceGroupsManagementService>(), configuration)
        {
            CallBase = true
        };
    }

    private static Response<QueryResult> QueryResponse(decimal amount, string? nextLink = null)
    {
        var columns = new List<QueryColumn>
        {
            ArmCostManagementModelFactory.QueryColumn("ResourceGroupName", "Dimension"),
            ArmCostManagementModelFactory.QueryColumn("Cost", "Number")
        };
        var rows = new List<IList<BinaryData>>
        {
            new List<BinaryData> { BinaryData.FromString("rg-test"), BinaryData.FromString(amount.ToString()) }
        };
        var result = ArmCostManagementModelFactory.QueryResult(nextLink: nextLink, columns: columns, rows: rows);
        return Response.FromValue(result, Substitute.For<Response>());
    }
}
