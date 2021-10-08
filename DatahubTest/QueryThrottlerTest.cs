using Datahub.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DatahubTest
{
    public class QueryThrottlerTest
    {
        [Fact]
        public async void QueryThrottler_CallbackIsInvoked()
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
        public async void QueryThrottler_CallbackWithNewerQuery()
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

            _ = Task.Run(() =>
            {
                target.SetQuery("dummy").Wait();
            });
            

            Thread.Sleep(TimeSpan.FromMilliseconds(100));

            await target.SetQuery(expected);
            
            Assert.Equal(actualCount, expectedCount);
            Assert.Equal(actual, expected);
        }

        [Fact]
        public async void QueryThrottler_CallbackWithBothQueries()
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
        public async void QueryThrottler_NullQueriesShouldBeIgnored()
        {
            var expected = string.Empty;
            var actual = string.Empty;
            var actualCount = 0;
            var expectedCount = 0;

            QueryThrottler<string> target = new(System.TimeSpan.FromMilliseconds(1), async (s) =>
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
}
