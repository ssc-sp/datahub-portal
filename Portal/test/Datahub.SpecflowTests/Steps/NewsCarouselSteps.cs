using Bunit;
using Datahub.Application.Services.Announcements;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Components;
using Datahub.Portal.Components.Announcements;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using GcdsWrapper.Blazor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor.Services;
using NSubstitute;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class NewsCarouselSteps(ScenarioContext scenarioContext) : BunitTestSteps
{
    private const string AnnouncementsContextKey = "announcements";
    private const string CarouselContextKey = "announcementCarousel";
    private const string CultureContextKey = "isFrench";
    private const string ServiceContextKey = "announcementService";

    [Given("there are no active announcement previews")]
    public void GivenThereAreNoActiveAnnouncementPreviews()
    {
        scenarioContext[AnnouncementsContextKey] = new List<AnnouncementPreview>();
    }

    [Given("the following active announcement previews")]
    public void GivenTheFollowingActiveAnnouncementPreviews(DataTable table)
    {
        scenarioContext[AnnouncementsContextKey] = table.Rows
            .Select((row, index) => new AnnouncementPreview(
                index + 1,
                row["Preview"].Replace("\\n", "\n"),
                int.Parse(row["Severity"])))
            .ToList();
    }

    [Given(@"an active announcement preview with severity (\d+)")]
    public void GivenAnActiveAnnouncementPreviewWithSeverity(int severity)
    {
        scenarioContext[AnnouncementsContextKey] = new List<AnnouncementPreview>
        {
            new(1, "Announcement title\nAnnouncement body", severity)
        };
    }

    [Given("an active image-only announcement preview")]
    public void GivenAnActiveImageOnlyAnnouncementPreview()
    {
        scenarioContext[AnnouncementsContextKey] = new List<AnnouncementPreview>
        {
            new(1, "![](/api/media/uploads/announcement.png)", 2)
        };
    }

    [Given("the current culture is French")]
    public void GivenTheCurrentCultureIsFrench()
    {
        scenarioContext[CultureContextKey] = true;
    }

    [When("the announcement carousel is rendered")]
    public void WhenTheAnnouncementCarouselIsRendered()
    {
        var announcements = scenarioContext.Get<List<AnnouncementPreview>>(AnnouncementsContextKey);
        var isFrench = scenarioContext.TryGetValue(CultureContextKey, out bool french) && french;

        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;

        var localizer = Substitute.For<IStringLocalizer>();
        localizer[Arg.Any<string>()]
            .Returns(call => new LocalizedString(call.ArgAt<string>(0), call.ArgAt<string>(0)));
        Services.AddSingleton(localizer);

        Services.AddSingleton<ICultureService>(new TestCultureService(isFrench));

        var announcementService = Substitute.For<IAnnouncementService>();
        announcementService.GetActivePreviews(isFrench).Returns(announcements);
        Services.AddSingleton(announcementService);
        scenarioContext[ServiceContextKey] = announcementService;

        scenarioContext[CarouselContextKey] = Render<AnnouncementCarousel>();
    }

    [Then("the announcement heading is not displayed")]
    public void ThenTheAnnouncementHeadingIsNotDisplayed()
    {
        GetCarousel().FindAll("h2").Should().BeEmpty();
    }

    [Then("the announcement heading is displayed")]
    public void ThenTheAnnouncementHeadingIsDisplayed()
    {
        GetCarousel().Find("h2").TextContent.Trim().Should().Be("Announcements");
    }

    [Then("no announcement notices are displayed")]
    public void ThenNoAnnouncementNoticesAreDisplayed()
    {
        GetCarousel().FindComponents<GcdsNotice>().Should().BeEmpty();
    }

    [Then(@"(\d+) announcement notices are displayed")]
    public void ThenAnnouncementNoticesAreDisplayed(int expectedCount)
    {
        GetCarousel().FindComponents<GcdsNotice>().Should().HaveCount(expectedCount);
    }

    [Then("the announcement titles are displayed in this order")]
    public void ThenTheAnnouncementTitlesAreDisplayedInThisOrder(DataTable table)
    {
        var expectedTitles = table.Rows.Select(row => row["Title"]);
        var actualTitles = GetCarousel().FindComponents<GcdsNotice>()
            .Select(notice => notice.Instance.NoticeTitle);

        actualTitles.Should().Equal(expectedTitles);
    }

    [Then("the warning announcement body is displayed")]
    public void ThenTheWarningAnnouncementBodyIsDisplayed()
    {
        var warningNotice = GetCarousel().FindComponents<GcdsNotice>()
            .Single(notice => notice.Instance.NoticeTitle == "Warning announcement");

        warningNotice.FindComponent<DHMarkdown>().Instance.Content.Should().Be("Warning body");
    }

    [Then("each announcement has a read more link")]
    public void ThenEachAnnouncementHasAReadMoreLink()
    {
        var notices = GetCarousel().FindComponents<GcdsNotice>();

        notices.Should().OnlyContain(notice =>
            notice.FindComponents<GcdsLink>().Count == 1 &&
            notice.FindComponent<GcdsLink>().Markup.Contains("Read more"));
    }

    [Then("the announcement notice role is {string}")]
    public void ThenTheAnnouncementNoticeRoleIs(string expectedRole)
    {
        GetCarousel().FindComponent<GcdsNotice>().Instance.NoticeRole.Should().Be(expectedRole);
    }

    [Then("French announcement previews are requested")]
    public void ThenFrenchAnnouncementPreviewsAreRequested()
    {
        var announcementService = scenarioContext.Get<IAnnouncementService>(ServiceContextKey);
        announcementService.Received(1).GetActivePreviews(true);
    }

    private IRenderedComponent<AnnouncementCarousel> GetCarousel()
    {
        return scenarioContext.Get<IRenderedComponent<AnnouncementCarousel>>(CarouselContextKey);
    }

    private sealed class TestCultureService(bool isFrench) : ICultureService
    {
        public string Culture { get; } = isFrench ? ICultureService.French : ICultureService.English;

        public ValueTask<string?> GetLanguageFromLocalStorageAsync()
        {
            return ValueTask.FromResult<string?>(Culture);
        }

        public Task SetLanguageInLocalStorageAsync(string language)
        {
            return Task.CompletedTask;
        }

        public void OverrideCurrentCulture(string cultureName)
        {
        }
    }
}
