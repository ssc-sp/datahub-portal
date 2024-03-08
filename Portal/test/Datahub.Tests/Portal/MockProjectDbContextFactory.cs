using System.Threading;
using System.Threading.Tasks;
using Datahub.Core.Model.Datahub;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Tests.Portal;

public class MockProjectDbContextFactory : IDbContextFactory<DatahubProjectDBContext>
{
	public DatahubProjectDBContext CreateDbContext()
	{
		var connection = new SqliteConnection("Data Source=InMemoryUnitTests;Mode=Memory;Cache=Shared");
		connection.Open();

		var options = new DbContextOptionsBuilder<DatahubProjectDBContext>()
			.UseSqlite(connection)
			.Options;

		var context = new DatahubProjectDBContext(options);
		context.Database.EnsureCreated();
		return context;
	}

	public Task<DatahubProjectDBContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
		=> Task.FromResult(CreateDbContext());
}

