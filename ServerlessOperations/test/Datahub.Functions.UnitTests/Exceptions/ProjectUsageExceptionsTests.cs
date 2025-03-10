using Datahub.Functions.Domain.Exceptions;
using FluentAssertions;

namespace Datahub.Functions.UnitTests
{
    [TestFixture]
    public class ProjectUsageExceptionsTests
    {
        [TestCase("Blob download error")]
        public void BlobDownloadException_ShouldSetMessage(string message)
        {
            var exception = new BlobDownloadException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Blob upload error")]
        public void BlobUploadException_ShouldSetMessage(string message)
        {
            var exception = new BlobUploadException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Cost update error")]
        public void CostUpdateException_ShouldSetMessage(string message)
        {
            var exception = new CostUpdateException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Cost refresh error")]
        public void CostRefreshException_ShouldSetMessage(string message)
        {
            var exception = new CostRefreshException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Budget update error")]
        public void BudgetUpdateException_ShouldSetMessage(string message)
        {
            var exception = new BudgetUpdateException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Rollover error")]
        public void RolloverException_ShouldSetMessage(string message)
        {
            var exception = new RolloverException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Cost query error")]
        public void CostQueryException_ShouldSetMessage(string message)
        {
            var exception = new CostQueryException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Project filtering error")]
        public void ProjectFilteringException_ShouldSetMessage(string message)
        {
            var exception = new ProjectFilteringException(message);
            message.Should().Be(exception.Message);
        }

        [TestCase("Message scheduling error")]
        public void MessageSchedulingException_ShouldSetMessage(string message)
        {
            var exception = new MessageSchedulingException(message);
            message.Should().Be(exception.Message);
        }
    }
}
