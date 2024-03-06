using Datahub.Core.Modules;
using Microsoft.Extensions.Configuration;
using System;
using Xunit;

namespace Datahub.Tests;

public class ModuleManagerTest
{
    private IConfigurationRoot _config;
    private FakeWebHostEnvironment _env;

    public ModuleManagerTest()
    {
        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile("appsettings.Development.json", false, true)
            .Build();
        _env = new FakeWebHostEnvironment();
    }

    [Fact(Skip = "Needs to be validated")]
    public void GivenModuleNameFindClass()
    {
        var mod = new ModuleManager();
        mod.LoadModules("*");

        //var objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
        //                   from type in asm.GetTypes()
        //                   where type.IsClass && type.Name == expectedClass
        //                  select type).Single();
        Assert.Contains(typeof(Datahub.Finance.FinanceModule), mod.Modules);
        Assert.Contains(typeof(Datahub.M365Forms.M365FormsModule), mod.Modules);
        Assert.Contains(typeof(Datahub.LanguageTraining.LanguageTrainingModule), mod.Modules);
    }

    [Fact(Skip = "Needs to be validated")]
    public void GivenFilterStarFindModule()
    {
        var mod = new ModuleManager();
        mod.LoadModules("*");
        Assert.Contains(typeof(Datahub.Finance.FinanceModule), mod.Modules);
        Assert.Contains(typeof(Datahub.M365Forms.M365FormsModule), mod.Modules);
        Assert.Contains(typeof(Datahub.LanguageTraining.LanguageTrainingModule), mod.Modules);
    }

    [Fact(Skip = "Needs to be validated")]
    public void GivenFilterSingleFindModule()
    {
        var mod = new ModuleManager();
        mod.LoadModules("*");

        Assert.Single(mod.Modules);
        Assert.Contains(typeof(Datahub.Finance.FinanceModule), mod.Modules);
    }

}