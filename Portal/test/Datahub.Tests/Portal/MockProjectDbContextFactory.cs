using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Achievements;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Datahub.Tests.Portal;

public class MockProjectDbContextFactory : IDbContextFactory<DatahubProjectDBContext>
{
    public DatahubProjectDBContext CreateDbContext()
    {
        var connection = new SqliteConnection("Data Source=InMemoryUnitTests;Mode=Memory;Cache=Shared");
        connection.Open();

        var options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
            .UseSqlite(connection)
            .Options;

        var context = new SqlServerDatahubContext(options);
        context.Database.EnsureCreated();
        context.TelemetryEvents = context.Set<TelemetryEvent>();

        return context;
    }

    public Task<DatahubProjectDBContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(CreateDbContext());
}

