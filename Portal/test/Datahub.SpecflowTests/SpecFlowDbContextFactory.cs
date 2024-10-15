using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.EntityFrameworkCore;

namespace Datahub.SpecflowTests;

public class SpecFlowDbContextFactory(DbContextOptions<DatahubProjectDBContext> options)
    : IDbContextFactory<DatahubProjectDBContext>
{
    public DatahubProjectDBContext CreateDbContext()
    {
        return new DatahubProjectDBContext(options);
    }

    public Task<DatahubProjectDBContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}
