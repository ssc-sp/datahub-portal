using Bunit;
using Datahub.Application.Configuration;
using Datahub.Application.Services.Announcements;
using Datahub.Infrastructure.Offline;
using Datahub.Portal.Components.Announcements;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Reqnroll;

namespace Datahub.SpecflowTests.Steps;

[Binding]
public sealed class NewsCarouselSteps(
    ScenarioContext scenarioContext,
    IWebHostEnvironment hostingEnvironment
    ) : TestContext
{
    // For additional details on SpecFlow step definitions see https://go.specflow.org/doc-stepdef

    
    private const string RelativePathToSrc = "../../../../../src";
    
    [Given(@"there is a news carousel component with an image")]
    public void GivenThereIsANewsCarouselComponentWithAnImage()
    {
        Services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
        var portalConfiguration = new DatahubPortalConfiguration()
        {
            CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
        };
        Services.AddDatahubOfflineInfrastructureServices(portalConfiguration);
        
        var module = JSInterop.SetupModule("./_content/Datahub.Core/Components/DHMarkdown.razor.js");
        JSInterop.Setup<BunitJSInterop>("import", "./_content/Datahub.Core/Components/DHMarkdown.razor.js")
            .SetResult(module);
        module.SetupVoid("styleCodeblocks");
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        
        var newsCarousel = RenderComponent<AnnouncementCarousel>(p => p
            .Add(p => p.Previews, new List<AnnouncementPreview>
            {
                new(1, @"![](/api/media//uploads/upload-11ade686-9bca-43e0-b449-caf4eb74d223.png)")
            })
        );
        scenarioContext["newsCarousel"] = newsCarousel;

     
    }

    [Then(@"the carousel should not have padding on the x-axis")]
    public void ThenTheCarouselShouldNotHavePaddingOnTheXAxis()
    {
        var newsCarousel = scenarioContext["newsCarousel"] as IRenderedComponent<AnnouncementCarousel>;
        newsCarousel!.Find(".mud-carousel-item.mud-carousel-item-default > div").ClassList.Should().NotContain("px-");
    }

    [Then(@"the carousel should not have padding on the y-axis")]
    public void ThenTheCarouselShouldNotHavePaddingOnTheYAxis()
    {
        var newsCarousel = scenarioContext["newsCarousel"] as IRenderedComponent<AnnouncementCarousel>;
        newsCarousel!.Find(".mud-carousel-item.mud-carousel-item-default > div").ClassList.Should().NotContain("py-");
    }

    [Given(@"there is a news carousel component without an image")]
    public void GivenThereIsANewsCarouselComponentWithoutAnImage()
    {
        Services.AddSingleton<IWebHostEnvironment>(hostingEnvironment);
        var portalConfiguration = new DatahubPortalConfiguration()
        {
            CultureSettings =
            {
                ResourcesPath = $"{RelativePathToSrc}/Datahub.Portal/i18n",
                AdditionalResourcePaths = []
            }
        };
        Services.AddDatahubOfflineInfrastructureServices(portalConfiguration);
        
        var module = JSInterop.SetupModule("./_content/Datahub.Core/Components/DHMarkdown.razor.js");
        JSInterop.Setup<BunitJSInterop>("import", "./_content/Datahub.Core/Components/DHMarkdown.razor.js")
            .SetResult(module);
        module.SetupVoid("styleCodeblocks");
        JSInterop.Mode = JSRuntimeMode.Loose;
        
        
        var newsCarousel = RenderComponent<AnnouncementCarousel>(p => p
            .Add(p => p.Previews, new List<AnnouncementPreview>
            {
                new(1, "## hello world")
            })
        );
        scenarioContext["newsCarousel"] = newsCarousel;
    }
    
    [Then(@"the carousel should have padding on the x-axis")]
    public void ThenTheCarouselShouldHavePaddingOnTheXAxis()
    {
        var newsCarousel = scenarioContext["newsCarousel"] as IRenderedComponent<AnnouncementCarousel>;
        newsCarousel!.Find(".mud-carousel-item.mud-carousel-item-default > div").ClassList.Should().Contain("px-12");
    }

    [Then(@"the carousel should have padding on the y-axis")]
    public void ThenTheCarouselShouldHavePaddingOnTheYAxis()
    {
        var newsCarousel = scenarioContext["newsCarousel"] as IRenderedComponent<AnnouncementCarousel>;
        newsCarousel!.Find(".mud-carousel-item.mud-carousel-item-default > div").ClassList.Should().Contain("py-4");
    }
}