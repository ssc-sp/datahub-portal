
namespace Datahub.Infrastructure.UnitTests.Services;

using static Testing;
public class UserEnrollmentServiceTests
{

    [Test]
    public async Task UserCanEnrollWithDataHubTest()
    {
        var result = await _userEnrollmentService.SendUserDatahubPortalInvite(TestUserEmail);
        
        Assert.That(result, Is.Not.Null.Or.Empty);
        Assert.That(result, Is.EqualTo(TestUserId));
    }

    [Test]
    [TestCase("fake@email.com")]
    [TestCase("fake@canada.com")]
    [TestCase("fake@gc.canada.ca")]
    public Task UserCanNotEnrollWithoutAGovernmentEmailTest(string email)
    {
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await _userEnrollmentService.SendUserDatahubPortalInvite(email);
        });
        return Task.CompletedTask;
    }
}