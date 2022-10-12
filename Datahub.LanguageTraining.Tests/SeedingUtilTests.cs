using Datahub.LanguageTraining.Data;
using System;
using System.Linq;
using Xunit;

namespace Datahub.LanguageTraining.Tests
{
    public class SeedingUtilTests
    {
        [Fact]
        public void SeedingUtil_Returns_Expected()
        {
            var entities = SeedingUtils.GetSeasonRegistrationPeriodSeeding(2022, 10).ToList();

            Assert.NotNull(entities);
            Assert.Equal(40, entities.Count);

            var expected = new DateTime(2022, 1, 17);
            var actual = entities[0].Open_DT;
            Assert.Equal(expected, actual);


            expected = new DateTime(2022, 10, 17);
            actual = entities[3].Open_DT;
            Assert.Equal(expected, actual);


            expected = new DateTime(2031, 12, 2);
            actual = entities[entities.Count - 1].Close_DT;
            Assert.Equal(expected, actual);
        }
    }
}