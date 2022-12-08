using Datahub.LanguageTraining.Utils;
using System;
using Xunit;

namespace Datahub.LanguageTraining.Tests;

public class TimeZoneTests
{
    [Fact]
    private void TestUtcToEstBackAndForeConversions()
    {
        var estMidnight = new DateTime(2022, 9, 22);
        var utcMidnight = estMidnight.ToUniversalTime();
        var actual = utcMidnight.ConvertUtcToEasternTime();
        Assert.Equal(actual, estMidnight);
    }
}