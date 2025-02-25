using Datahub.Functions.Validators;
using Datahub.Infrastructure.Queues.Messages; 
using FluentAssertions;
using FluentValidation.TestHelper; 

namespace Datahub.Functions.UnitTests.Validators
{
    [TestFixture]
    public class EmailValidatorTests
    {
        private EmailValidator _emailValidator;

        [SetUp]
        public void SetUp()
        {
            _emailValidator = new EmailValidator();
        }

        [Test]
        [TestCase("test@example.com", true)]
        [TestCase("user.name@example.co.uk", true)]
        [TestCase("user_name@example.com", true)]
        [TestCase("user-name@example.com", true)]
        [TestCase("user@example", false)]
        [TestCase("user@.com", false)]
        [TestCase("user@com", false)]
        [TestCase("user@com.", false)]
        [TestCase("user@com..com", false)]
        [TestCase("user@.com.com", false)]
        [TestCase("user@com@com.com", false)]
        [TestCase("user@com@com", false)]
        [TestCase("user@com..com", false)]
        [TestCase("user@com.com.", false)]
        [TestCase("user@com.com..", false)]
        [TestCase("user@com.com..com", false)]
        [TestCase("", false)]
        public void IsValidEmail_ShouldReturnExpectedResult(string email, bool expectedResult)
        {
            // Act
            var result = _emailValidator.IsValidEmail(email);

            // Assert
            result.Should().Be(expectedResult);
        }
    }

    [TestFixture]
    public class ProjectUsageUpdateMessageValidatorTests
    {
        private ProjectUsageUpdateMessageValidator _validator;
         
        [SetUp]
        public void SetUp()
        {
            _validator = new ProjectUsageUpdateMessageValidator(); 
        }

        [Test]
        public void Should_Have_Error_When_ProjectAcronym_Is_Empty()
        {
            // Arrange
            var message = new ProjectUsageUpdateMessage("", "blob.json", "totals.json", false);

            // Act
            var result = _validator.TestValidate(message);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.ProjectAcronym);
        }

        [Test]
        public void Should_Have_Error_When_CostsBlobName_Is_Empty()
        {
            // Arrange
            var message = new ProjectUsageUpdateMessage("PA", "", "totals.json", false);

            // Act
            var result = _validator.TestValidate(message);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CostsBlobName);
        }

        [Test]
        public void Should_Have_Error_When_CostsBlobName_Does_Not_End_With_Json()
        {
            // Arrange
            var message = new ProjectUsageUpdateMessage("PA", "blob.txt", "totals.json", false);

            // Act
            var result = _validator.TestValidate(message);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CostsBlobName);
        }

        [Test]
        public void Should_Not_Have_Error_When_Message_Is_Valid()
        {
            // Arrange
            var message = new ProjectUsageUpdateMessage("PA", "blob.json", "totals.json", false);

            // Act
            var result = _validator.TestValidate(message);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.ProjectAcronym);
            result.ShouldNotHaveValidationErrorFor(x => x.CostsBlobName);
        }
    }
}
