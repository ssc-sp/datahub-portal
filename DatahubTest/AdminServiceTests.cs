using Datahub.Core.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Datahub.Tests
{
    public class AdminServiceTests
    {
        private IDbContextFactory<DatahubProjectDBContext> dbFactory;

        public AdminServiceTests()
        {
            var ctx = new DatahubProjectDBContext(new DbContextOptionsBuilder<DatahubProjectDBContext>()
                    .UseInMemoryDatabase("InMemoryTest")
                    .Options);
            var mockDbFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
            mockDbFactory.Setup(f => f.CreateDbContext())
                .Returns(ctx);
            mockDbFactory.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(new DatahubProjectDBContext(new DbContextOptionsBuilder<DatahubProjectDBContext>()
                    .UseInMemoryDatabase("InMemoryTest")
                    .Options)));
            dbFactory = mockDbFactory.Object;
        }

        [Fact]
        public async Task IdentifyDuplicates()
        {
            using var ctx = await dbFactory.CreateDbContextAsync();
            var lst = await ctx.Project_Users_Requests
                .GroupBy(a => new { a.User_ID, a.Project.Project_ID })
                .Where(gp => gp.Count() > 1)
                .Select(gp => gp.ToList())
                .ToListAsync();
            //var dups = await ctx.Project_Users_Requests.GroupBy(a => new { a.Project, a.User_ID }).SelectMany(grp => grp.Skip(1)).ToListAsync();

        }
    }
}
