using Datahub.Core.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests;

public class QueryThrottlerTest
{
    [Fact]
    public async void QueryThrottlerCallbackIsInvoked()
    {
        var expected = "expected query";
        var actual = string.Empty;

        QueryThrottler<string> target = new(TimeSpan.FromMilliseconds(1), async (s) =>
        {
            actual = s;
            await Task.FromResult(0);
        });

        await target.SetQuery(expected);

        Assert.Equal(actual, expected);
    }

    [Fact]
    public async void QueryThrottlerCallbackWithNewerQuery()
    {
        var expected = "expected query";
        var actual = string.Empty;
        var expectedCount = 1;
        var actualCount = 0;

        QueryThrottler<string> target = new(TimeSpan.FromMilliseconds(200), async (s) =>
        {
            actual = s;
            actualCount++;
            await Task.FromResult(0);
        });

        _ = Task.Run(async () =>
        {
            await target.SetQuery("dummy");
        });


        Thread.Sleep(TimeSpan.FromMilliseconds(100));

        await target.SetQuery(expected);

        Assert.Equal(actualCount, expectedCount);
        Assert.Equal(actual, expected);
    }

    [Fact]
    public async void QueryThrottlerCallbackWithBothQueries()
    {
        var expected = "expected query";
        var actual = string.Empty;
        var expectedCount = 2;
        var actualCount = 0;

        QueryThrottler<string> target = new(TimeSpan.FromMilliseconds(200), async (s) =>
        {
            actual = s;
            actualCount++;
            await Task.FromResult(0);
        });

        _ = target.SetQuery("dummy");

        Thread.Sleep(TimeSpan.FromMilliseconds(250));

        await target.SetQuery(expected);

        Assert.Equal(actualCount, expectedCount);
        Assert.Equal(actual, expected);
    }

    [Fact]
    public async void QueryThrottlerNullQueriesShouldBeIgnored()
    {
        var expected = string.Empty;
        var actual = string.Empty;
        var actualCount = 0;
        var expectedCount = 0;

        QueryThrottler<string> target = new(TimeSpan.FromMilliseconds(1), async (s) =>
        {
            actual = s;
            actualCount++;
            await Task.FromResult(0);
        });

        await target.SetQuery(null);

        Assert.Equal(actualCount, expectedCount);
        Assert.Equal(actual, expected);
    }
}