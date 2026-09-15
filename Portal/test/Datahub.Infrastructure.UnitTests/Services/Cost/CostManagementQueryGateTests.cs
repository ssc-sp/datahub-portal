using Datahub.Infrastructure.Services.Cost;
using FluentAssertions;
using System.Diagnostics;

namespace Datahub.Infrastructure.UnitTests.Services.Cost;

[TestFixture]
public class CostManagementQueryGateTests
{
    [Test]
    public async Task WaitAsync_ShouldApplyMinimumIntervalBetweenRequests()
    {
        // Arrange
        var minimumInterval = TimeSpan.FromMilliseconds(75);
        var sut = new CostManagementQueryGate(minimumInterval);
        await sut.WaitAsync(CancellationToken.None);
        var startedAt = Stopwatch.GetTimestamp();

        // Act
        await sut.WaitAsync(CancellationToken.None);

        // Assert
        Stopwatch.GetElapsedTime(startedAt).Should().BeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(60));
    }

    [Test]
    public async Task WaitAsync_ShouldSupportCancellation()
    {
        // Arrange
        var sut = new CostManagementQueryGate(TimeSpan.FromSeconds(10));
        await sut.WaitAsync(CancellationToken.None);
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMilliseconds(25));

        // Act
        var action = () => sut.WaitAsync(cancellationTokenSource.Token);

        // Assert
        await action.Should().ThrowAsync<OperationCanceledException>();
    }
}
