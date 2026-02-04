using System;
using System.Threading.Tasks;
using Datahub.Application.Services.Security;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Datahub.Tests;

public class ServiceAuthTests:IDisposable
{
    private readonly SqlServerDatahubContext ctx;
    private IServiceAuthManager _authManager;

    public ServiceAuthTests()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        var serviceProvider = services.BuildServiceProvider();

        var serviceAuthCache = serviceProvider.GetRequiredService<IMemoryCache>();
        var mockDbFactory = new Mock<IDbContextFactory<DatahubProjectDBContext>>();
        ctx = new SqlServerDatahubContext(new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseInMemoryDatabase("InMemoryTest")
            .Options);
        mockDbFactory.Setup(f => f.CreateDbContext())
            .Returns(ctx);
        _authManager = new ServiceAuthManager(serviceAuthCache, mockDbFactory.Object, NullLogger<ServiceAuthManager>.Instance);
    }

    public void Dispose()
    {
        ctx.Dispose();
    }

    [Fact (Skip = "Needs to be validated")]
    public async Task GivenUser_RetrieveProjects()
    {
        var auths = await _authManager.GetEntraUserAuthorizations("d6d53fcc-9d82-4b0e-8b91-91248c344224");
    }
}
