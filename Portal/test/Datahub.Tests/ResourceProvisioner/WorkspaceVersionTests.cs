using System.Collections.Generic;
using System.Linq;
using Xunit;
using System;
using System.Collections;
using System.Threading.Tasks;
using Datahub.Application.Services;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data.ResourceProvisioner;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Datahub.Core.Services;
using Datahub.Infrastructure.Offline;
using Datahub.Infrastructure.Services;
using Foundatio.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Datahub.Core.Model.Context;

namespace Datahub.Tests.ResourceProvisioner;

public class WorkspaceVersionTests
{
    private IConfiguration _config;
   
    private ServiceProvider SetupServices()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddPooledDbContextFactory<DatahubProjectDBContext>(options => options.UseInMemoryDatabase("datahubProjects"));
        services.AddScoped<IWorkspaceVersionService, WorkspaceVersionService>();        
        //dependency for ProjectCreationService
        services.AddSingleton(Configuration);        
        services.AddScoped<IUserInformationService, OfflineUserInformationService>();
        return services.BuildServiceProvider();
    }


    [Fact]
    public async Task GivenNewVersionBuild_FlagVersionsWithSameMajorMinorAndLowerBuild()
    {
        var versions = new Dictionary<string, bool>
       {
          { "v1.0.0", false },
          { "v1.0.1", false },
          { "v2.0.0", false },
          { "v2.1.1", true },
          { "v2.1.0", true },
          { "v3.0.0", false },
          { "v3.1.0", false }
       };

        var newVersion = "v2.1.6";
        var parsedNewVersion = Version.Parse(newVersion.TrimStart('v'));       

        Assert.All(versions, kvp =>
        {
            var parsedKeyVersion = Version.Parse(kvp.Key.TrimStart('v'));
            if (parsedKeyVersion.Major == parsedNewVersion.Major &&
                parsedKeyVersion.Minor == parsedNewVersion.Minor &&
                parsedKeyVersion.Build < parsedNewVersion.Build)
            {
                Assert.True(kvp.Value, $"Version {kvp.Key} should be flagged as true.");
            }
            else
            {
                Assert.False(kvp.Value, $"Version {kvp.Key} should not be flagged as true.");
            }
        });
    }

    [Fact]
    public async Task GivenMultipleVersions_GetPreviousBuildIfExists()
    {
        var testCases = new Dictionary<string, string>
       {
           { "v2.0.1", "v2.0.0" },
           { "v3.1.0", string.Empty },
           { "v1.0.10", "v1.0.9" },
           { "v4.2.3", "v4.2.2" }
       };

        foreach (var testCase in testCases)
        {
            var version = testCase.Key;
            var expectedPreviousVersion = testCase.Value;

            var parsedVersion = Version.Parse(version.TrimStart('v'));
            string previousVersion = parsedVersion.Build > 0
                ? $"v{parsedVersion.Major}.{parsedVersion.Minor}.{parsedVersion.Build - 1}"
                : string.Empty;

            Assert.True(previousVersion == expectedPreviousVersion,
                $"For version {version}, expected previous version to be {expectedPreviousVersion} but was {previousVersion}");
        }
    }

    [Fact]
    public async Task GivenNewVersion_CheckIfGreenLightChangesRequired()
    {
        var newVersionbuild = "v2.0.1";
        var newVersionminor = "v2.1.0";
        var newVersionmajor = "v3.0.0";

        var newVersions = new List<string> { newVersionbuild, newVersionminor, newVersionmajor };

        var existingVersions = new Dictionary<string, (bool, bool, bool)>()
        {
            { "v1.4", (false, false, false) },
            { "v8.3.6", (false, false, false) },
            { "v2.0.0", (true, false, false) },
            { "v10.3.4", (false, false, false) },
            { "v3.0.0", (false, false, false) },
            { "v2.1.0", (false, false, false) }
        };

        foreach (var version in existingVersions)
        {
            for (int i = 0; i < newVersions.Count; i++)
            {
                var shouldUpdate = false;
                var newVersion = newVersions[i];
                var newVer = Version.Parse(newVersion.TrimStart('v'));
                var existingVer = Version.Parse(version.Key.TrimStart('v'));

                if (newVer.Major == existingVer.Major && newVer.Minor == existingVer.Minor && newVer.Build > existingVer.Build)
                {
                    shouldUpdate = true;
                }

                var expectedValue = i switch
                {
                    0 => version.Value.Item1,
                    1 => version.Value.Item2,
                    2 => version.Value.Item3,
                    _ => throw new InvalidOperationException("Unexpected iteration index")
                };

                Assert.Equal(expectedValue, shouldUpdate);
            }

        }
    }

    [Fact]
    public async Task GivenListOfVersionTagsThenGetLatestVersion()
    {
        var versions = new List<string> { "v1.4", "v8.3.6", "v2.0.1", "v10.3.4", "v4.13.6", "v3.0.1" };

        var latest = versions
           .Select(v => Version.Parse(v.TrimStart('v')))
           .OrderByDescending(v => v)
           .First();

        Assert.True(latest.ToString() == "10.3.4", $"Latest version should be 10.3.4 but was {latest}");

    }

    private IConfiguration Configuration
    {
        get
        {
            if (_config != null) return _config;
            var builder = new ConfigurationBuilder().AddJsonFile($"testsettings.json", optional: false);
            _config = builder.Build();

            return _config;
        }
    }
}