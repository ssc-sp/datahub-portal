using AspNetCore.Localizer.Json.Commons;
using AspNetCore.Localizer.Json.Extensions;
using AspNetCore.Localizer.Json.JsonOptions;
using Bunit;
using Bunit.TestDoubles;
using Datahub.Portal;
using Datahub.SpecflowTests.Utils;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MudBlazor;
using Reqnroll;
using System.Globalization;
using System.Text;

namespace Datahub.SpecflowTests.Steps;

[Binding]
internal class LocalizerSteps(
    ScenarioContext scenarioContext,
    IWebHostEnvironment hostingEnvironment
)
{

    private static IStringLocalizer CreateLocalizer()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddJsonLocalization(options =>
        {
            options.CacheDuration = TimeSpan.FromMinutes(15);
            options.ResourcesPath = "i18n";
            options.LocalizationMode = LocalizationMode.I18n;
            options.UseEmbeddedResources = true;
            options.MissingTranslationLogBehavior = MissingTranslationLogBehavior.Ignore;
            options.FileEncoding = Encoding.GetEncoding("UTF-8");
            options.SupportedCultureInfos = new HashSet<CultureInfo>
            {
                new("en-CA"),
                new("fr-CA")
            };
            options.AssemblyHelper = new AssemblyHelper(typeof(Startup).Assembly);
        });

        using var serviceProvider = services.BuildServiceProvider();
        var factory = serviceProvider.GetRequiredService<IStringLocalizerFactory>();
        return factory.Create(typeof(LocalizerSteps));
    }

    [Given("a text field with localized content for (.*) from (.*)")]
    public void GivenATextFieldWithLocalizedContentForFrom(string key, string file)
    {
        scenarioContext["Key"] = key;
        scenarioContext["File"] = file;
    }

    [When("a user views the text in (.*)")]
    public void WhenAUserViewsTheTextIn(string language)
    {
        scenarioContext["Language"] = language;

        var cultureInfo = language.ToLowerInvariant() switch
        {
            "en" or "en-ca" => new CultureInfo("en-CA"),
            "fr" or "fr-ca" => new CultureInfo("fr-CA"),
            _ => throw new ArgumentException($"Unsupported language: {language}")
        };

        scenarioContext["LanguageKey"] = cultureInfo.Name;
        Thread.CurrentThread.CurrentUICulture = cultureInfo;
        Thread.CurrentThread.CurrentCulture = cultureInfo;

        var key = scenarioContext["Key"] as string;
        var stringLocalizer = CreateLocalizer();
        var localizedString = stringLocalizer[key!];
        scenarioContext["Output"] = localizedString.Value;
    }

    [Then("the user should see localized text (.*)")]
    public void ThenTheUserShouldSeeLocalizedText(string expectedoutput)
    {
        scenarioContext.TryGetValue("Output", out var outputObj).Should().BeTrue("the scenario should set Output in the When step");
        outputObj.Should().BeOfType<string>("Output should be stored as a string in the scenario context");
        var actualOutput = (string)outputObj!;
        var file = scenarioContext["File"] ?? string.Empty;
        actualOutput.Should().Be(expectedoutput, $"that is what is expected in {file}");
    }
}
