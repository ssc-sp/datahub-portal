using Xunit;

namespace Datahub.Tests.Migrations;

/// <summary>
/// xUnit collection used to serialize DB migration tests and prevent them from running in parallel.
/// </summary>
[CollectionDefinition("DbMigrations", DisableParallelization = true)]
public class DbMigrationsCollection { }
