using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Xunit;
using Datahub.Core.Model.Context;
using NSubstitute;

namespace Datahub.Tests;

public class AdminServiceTests
{
    private IDbContextFactory<DatahubProjectDBContext> dbFactory;

    public AdminServiceTests()
    {
        var ctx = new SqlServerDatahubContext(new DbContextOptionsBuilder<SqlServerDatahubContext>()
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

    [Fact]
    public void CheckClaims()
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "Offline User"),
            new Claim(ClaimTypes.Role, "default"),
        }, "Fake authentication type");

        var user = new ClaimsPrincipal(identity);

        var totalRoles = user.Claims.Where(c => c.Type == ClaimTypes.Role).Count();
        //var claim = user.Claims.Where(c => c.Type == ClaimTypes.Role && c.Value == "default").FirstOrDefault();

        var claim = user.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();

        Assert.True(user is not null);
        Assert.True(claim.Count() == 1);
        Assert.True(claim[0].Value == "default");
    }
}