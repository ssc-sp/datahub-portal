using System;
using System.Collections.Generic;
using Datahub.Shared.Entities;
using Datahub.Shared.Entities.WorkspaceToolConfiguration;
using Xunit;

namespace Datahub.Tests;

public class WorkspaceToolConfigurationTests
{
    public static IEnumerable<object[]> ConfigurationCases()
    {
        yield return
        [
            typeof(AppServiceConfiguration),
            new AppServiceConfiguration
            {
                Framework = AppServiceTemplates.CUSTOM,
                GitRepo = "https://example.com/base.git",
                ComposePath = "docker-compose.yml"
            },
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.AppServiceConfiguration = (AppServiceConfiguration)config)
        ];

        yield return
        [
            typeof(AppServiceConfigurationEnhanced),
            new AppServiceConfigurationEnhanced
            {
                Framework = AppServiceTemplates.CUSTOM,
                GitRepo = "https://example.com/enhanced.git",
                ComposePath = "docker-compose.yml"
            },
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.AppServiceConfiguration = (AppServiceConfiguration)config)
        ];

        yield return
        [
            typeof(PostgresConfiguration),
            new PostgresConfiguration { PSQL_SKU = PostgresTier.DefaultTier.PSQL_SKU },
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.PostgresConfiguration = (PostgresConfiguration)config)
        ];

        yield return
        [
            typeof(PostgresConfigurationEnhanced),
            new PostgresConfigurationEnhanced { PSQL_SKU = PostgresTier.DefaultTier.PSQL_SKU },
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.PostgresConfiguration = (PostgresConfiguration)config)
        ];

        yield return
        [
            typeof(DatabricksConfiguration),
            new DatabricksConfiguration(),
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.DatabricksConfiguration = (DatabricksConfiguration)config)
        ];

        yield return
        [
            typeof(DatabricksConfigurationEnhanced),
            new DatabricksConfigurationEnhanced(),
            (Action<WorkspaceAppData, IWorkspaceToolConfiguration>)((appData, config) =>
                appData.DatabricksConfiguration = (DatabricksConfiguration)config)
        ];
    }

    [Theory]
    [MemberData(nameof(ConfigurationCases))]
    public void ReadFromWorkspaceDefinition_ReturnsExistingConfiguration(
        Type configurationType,
        IWorkspaceToolConfiguration existingConfiguration,
        Action<WorkspaceAppData, IWorkspaceToolConfiguration> setConfiguration)
    {
        var workspaceDefinition = CreateWorkspaceDefinition();
        setConfiguration(workspaceDefinition.AppData, existingConfiguration);

        var configInfo = new VersionAwareWorkspaceToolConfigInfo
        {
            ConfigClass = configurationType
        };

        var result = configInfo.ReadFromWorkspaceDefinition(workspaceDefinition);

        Assert.Same(existingConfiguration, result);
    }

    [Theory]
    [MemberData(nameof(ConfigurationCases))]
    public void ReadFromWorkspaceDefinition_ReturnsConfigurationOfConfiguredTypeWhenMissing(
        Type configurationType,
        IWorkspaceToolConfiguration _,
        Action<WorkspaceAppData, IWorkspaceToolConfiguration> setConfiguration)
    {
        var workspaceDefinition = CreateWorkspaceDefinition();
        setConfiguration(workspaceDefinition.AppData, null!);

        var configInfo = new VersionAwareWorkspaceToolConfigInfo
        {
            ConfigClass = configurationType
        };

        var result = configInfo.ReadFromWorkspaceDefinition(workspaceDefinition);

        Assert.IsType(configurationType, result);
    }

    [Fact]
    public void GetPropertyLabel_ReturnsLabelsFromConfiguredConfigurationTypes()
    {
        var databricksInfo = new VersionAwareWorkspaceToolConfigInfo
        {
            ConfigClass = typeof(DatabricksConfigurationEnhanced)
        };

        Assert.Equal("ML tier", databricksInfo.GetPropertyLabel(nameof(DatabricksConfiguration.MachineLearningTierSku)));

        var postgresInfo = new VersionAwareWorkspaceToolConfigInfo
        {
            ConfigClass = typeof(PostgresConfigurationEnhanced)
        };

        Assert.Equal("Database tier", postgresInfo.GetPropertyLabel(nameof(PostgresConfiguration.PSQL_SKU)));

        var appServiceInfo = new VersionAwareWorkspaceToolConfigInfo
        {
            ConfigClass = typeof(AppServiceConfigurationEnhanced)
        };

        Assert.Equal("Framework", appServiceInfo.GetPropertyLabel("Framework"));
    }

    private static WorkspaceDefinition CreateWorkspaceDefinition() => new()
    {
        Templates = [],
        Workspace = new TerraformWorkspace(),
        AppData = new WorkspaceAppData(),
        RequestingUserEmail = "test@example.com"
    };
}
