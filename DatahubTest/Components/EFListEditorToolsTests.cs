using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NRCan.Datahub.Shared.EFCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DatahubTest.Components
{
    public class EFListEditorToolsTests
    {


        private IEnumerable<T> LoadCollectionGeneric<S,T>(ServiceProvider provider, Func<S, IEnumerable> loadSource) where S:DbContext
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
                ctx.Projects.Add(new Datahub_Project() { Project_ID = 1, Project_Name = "Test1", Project_Acronym_CD = "T1" });
                ctx.Projects.Add(new Datahub_Project() { Project_ID = 2, Project_Name = "Test2", Project_Acronym_CD = "T2" });
                ctx.SaveChanges();
            }
            var data = LoadCollectionGeneric<DatahubProjectDBContext, Datahub_Project>(sp,e => e.Projects);
            Assert.Equal(2, data.Count());
        }
    }
}
