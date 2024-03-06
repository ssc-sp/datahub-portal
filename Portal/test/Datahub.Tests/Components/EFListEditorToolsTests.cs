using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Model.Projects;
using Xunit;

namespace Datahub.Tests.Components;

public class EFListEditorToolsTests
{


    private IEnumerable<T> LoadCollectionGeneric<S, T>(ServiceProvider provider, Func<S, IEnumerable> loadSource) where S : DbContext
    {
        //Expression<Func<S, IEnumerable>> expression = d => d.Projects;
        //Func<S, IEnumerable> loadSource = d => d.Projects;
        //IDbContextFactory
        var fac = provider.GetRequiredService<IDbContextFactory<S>>();
        using var ctx = fac.CreateDbContext();
        return (loadSource(ctx) as IEnumerable<T>).ToList();
    }

    private static ServiceProvider SetupServices<S>() where S : DbContext
    {
        var services = new ServiceCollection();
        services.AddPooledDbContextFactory<S>(options => options.UseInMemoryDatabase("datahubProjects"));
        var provider = services.BuildServiceProvider();
        return provider;
    }

    [Fact]
    public void TestLoadSource()
    {
        var sp = SetupServices<DatahubProjectDBContext>();
        var fac = sp.GetRequiredService<IDbContextFactory<DatahubProjectDBContext>>();
        using (var ctx = fac.CreateDbContext())
        {
            ctx.Projects.Add(new DatahubProject() { ProjectID = 1, Project_Name = "Test1", ProjectAcronymCD = "T1" });
            ctx.Projects.Add(new DatahubProject() { ProjectID = 2, Project_Name = "Test2", ProjectAcronymCD = "T2" });
            ctx.SaveChanges();
        }
        var data = LoadCollectionGeneric<DatahubProjectDBContext, DatahubProject>(sp, e => e.Projects);
        Assert.Equal(2, data.Count());
    }
}