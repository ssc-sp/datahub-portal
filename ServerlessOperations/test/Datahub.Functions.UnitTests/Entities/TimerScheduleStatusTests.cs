using System;
using FluentAssertions;
using NUnit.Framework;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class TimerScheduleStatusTests
    {
        [Test]
        public void TimerScheduleStatus_ShouldInitializeWithDefaultValues()
        {
            // Arrange & Act
            var timerScheduleStatus = new TimerScheduleStatus();

            // Assert
            timerScheduleStatus.Last.Should().Be(default(DateTime));
            timerScheduleStatus.Next.Should().Be(default(DateTime));
            timerScheduleStatus.LastUpdated.Should().Be(default(DateTime));
        }

        [Test]
        public void TimerScheduleStatus_ShouldSetAndGetProperties()
        {
            // Arrange
            var last = new DateTime(2023, 1, 1);
            var next = new DateTime(2023, 2, 1);
            var lastUpdated = new DateTime(2023, 1, 15);

            // Act
            var timerScheduleStatus = new TimerScheduleStatus
            {
                Last = last,
                Next = next,
                LastUpdated = lastUpdated
            };

            // Assert
            timerScheduleStatus.Last.Should().Be(last);
            timerScheduleStatus.Next.Should().Be(next);
            timerScheduleStatus.LastUpdated.Should().Be(lastUpdated);
        }

        [Test]
        public void TimerScheduleStatus_ShouldUpdateLastProperty()
        {
            // Arrange
            var timerScheduleStatus = new TimerScheduleStatus();
            var newLast = new DateTime(2023, 3, 1);

            // Act
            timerScheduleStatus.Last = newLast;

            // Assert
            timerScheduleStatus.Last.Should().Be(newLast);
        }

        [Test]
        public void TimerScheduleStatus_ShouldUpdateNextProperty()
        {
            // Arrange
            var timerScheduleStatus = new TimerScheduleStatus();
            var newNext = new DateTime(2023, 4, 1);

            // Act
            timerScheduleStatus.Next = newNext;

            // Assert
            timerScheduleStatus.Next.Should().Be(newNext);
        }

        [Test]
        public void TimerScheduleStatus_ShouldUpdateLastUpdatedProperty()
        {
            // Arrange
            var timerScheduleStatus = new TimerScheduleStatus();
            var newLastUpdated = new DateTime(2023, 5, 1);

            // Act
            timerScheduleStatus.LastUpdated = newLastUpdated;

            // Assert
            timerScheduleStatus.LastUpdated.Should().Be(newLastUpdated);
        }
    }
}

