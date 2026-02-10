using System;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Subscriptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Datahub.Tests.Migrations;

[Collection("DbMigrations")] // ensure serialized with other DB migration tests
/// <summary>
/// Migration and EF seeding tests specific to the Core (DatahubProjectDBContext) database.
/// </summary>
public class CoreDbMigrationTests : IAsyncLifetime
{
 private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(b => b.AddConsole());
 private string _coreDbName = string.Empty;
 private DbContextOptions<SqlServerDatahubContext> _options = null!;
 private bool _skip;

 public ValueTask InitializeAsync()
 {
 if (!OperatingSystem.IsWindows()) { _skip = true; return ValueTask.CompletedTask; }
 if (!LocalDbUtils.IsLocalDbAvailable()) { _skip = true; return ValueTask.CompletedTask; }
 _coreDbName = LocalDbUtils.TryCreateUniqueLocalDbDatabase("CoreDb") ?? string.Empty;
 if (string.IsNullOrWhiteSpace(_coreDbName)) { _skip = true; return ValueTask.CompletedTask; }
 _options = new DbContextOptionsBuilder<SqlServerDatahubContext>()
 .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={_coreDbName};Integrated Security=true;TrustServerCertificate=true;")
 .UseLoggerFactory(_loggerFactory)
 .Options;
 return ValueTask.CompletedTask;
 }

 public ValueTask DisposeAsync()
 {
 if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(_coreDbName))
 {
 LocalDbUtils.DropLocalDbDatabase(_coreDbName);
 }
 return ValueTask.CompletedTask;
 }

 [Fact]
 public async Task CoreDb_Migrate_From_First_With_EF_Seed_To_Latest()
 {
 if (_skip) return;

 //1) migrate to first migration
 await using (var ctx = new SqlServerDatahubContext(_options))
 {
 var first = ctx.Database.GetMigrations().First();
 await ctx.Database.MigrateAsync(first);
 }

 //2) seed minimal data using EF
 await using (var ctx = new SqlServerDatahubContext(_options))
 {
 ctx.AzureSubscriptions.Add(new DatahubAzureSubscription
 {
 SubscriptionId = "00000000-0000-0000-0000-000000000000",
 TenantId = "00000000-0000-0000-0000-000000000000",
 SubscriptionName = "Local"
 });
 await ctx.SaveChangesAsync();
 }

 //3) migrate to latest and validate
 await using (var ctx = new SqlServerDatahubContext(_options))
 {
 await ctx.Database.MigrateAsync();
 var count = await ctx.AzureSubscriptions.CountAsync();
 Assert.True(count >=1);
 }
 }
}
