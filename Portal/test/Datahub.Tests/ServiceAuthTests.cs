using Datahub.Core.Services.Security;
using Datahub.Core.Services.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Xunit;

namespace Datahub.Tests;

public class ServiceAuthTests:IDisposable
{
    private readonly DatahubProjectDBContext ctx;
    private ServiceAuthManager _authManager;

    public ServiceAuthTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        var serviceAuthCache = serviceProvider.GetService<IMemoryCache>();
        var mockDbFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        ctx = new DatahubProjectDBContext(new DbContextOptionsBuilder<DatahubProjectDBContext>()
            .UseInMemoryDatabase("InMemoryTest")
            .Options);
        mockDbFactory.Setup(f => f.CreateDbContext())
            .Returns(ctx);
        _authManager = new ServiceAuthManager(serviceAuthCache, mockDbFactory.Object);
    }

    public void Dispose()
    {
        ctx.Dispose();
    }

    [Fact (Skip = "Needs to be validated")]
    public async Task GivenUser_RetrieveProjects()
    {
        var auths = await _authManager.GetUserAuthorizations("d6d53fcc-9d82-4b0e-8b91-91248c344224");
    }
}