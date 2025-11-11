using System;
using System.Linq;
using System.Threading.Tasks;
using Datahub.Core.Model.Context;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Datahub.Tests.Migrations;

[Collection("DbMigrations")] // ensure serialized with other DB migration tests
/// <summary>
/// Migration and EF seeding tests specific to the Metadata database.
/// </summary>
public class MetadataDbMigrationTests : IAsyncLifetime
{
    private readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(b => b.AddConsole());
    private string _metaDbName = string.Empty;
    private DbContextOptions<SqlServerMetadataDbContext> _options = null!;

    public ValueTask InitializeAsync()
    {
        if (!OperatingSystem.IsWindows()) return ValueTask.CompletedTask; // LocalDB only on Windows
        _metaDbName = LocalDbUtils.CreateUniqueLocalDbDatabase("MetaDb");
        _options = new DbContextOptionsBuilder<SqlServerMetadataDbContext>()
                        .UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={_metaDbName};Integrated Security=true;TrustServerCertificate=true;")
                        .UseLoggerFactory(_loggerFactory)
                        .Options;
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        if (OperatingSystem.IsWindows() && !string.IsNullOrEmpty(_metaDbName))
        {
            LocalDbUtils.DropLocalDbDatabase(_metaDbName);
        }
        return ValueTask.CompletedTask;
    }

    [Fact]
    public async Task MetadataDb_Migrate_From_Compatible_With_EF_Seed_To_Latest()
    {
        if (!OperatingSystem.IsWindows()) return; // skip on non-Windows

        //1) migrate to a migration compatible with current model for FieldDefinitions/MetadataVersions
        await using (var ctx = new SqlServerMetadataDbContext(_options))
        {
            var migrations = ctx.Database.GetMigrations().ToList();
            var target = migrations.FirstOrDefault(m => m.Contains("20220118174527") || m.Contains("AddingCascadingValueField"))
            ?? migrations.First();
            await ctx.Database.MigrateAsync(target);
        }

        //2) seed using EF
        await using (var ctx = new SqlServerMetadataDbContext(_options))
        {
            var version = new MetadataVersion
            {
                Source_TXT = "test",
                Last_Update_DT = DateTime.UtcNow,
                Version_Info_TXT = "v0"
            };
            ctx.MetadataVersions.Add(version);
            await ctx.SaveChangesAsync();

            var def = new FieldDefinition
            {
                MetadataVersionId = version.MetadataVersionId,
                Field_Name_TXT = "TestField",
                Sort_Order_NUM = 0,
                Required_FLAG = false,
                MultiSelect_FLAG = false,
                Custom_Field_FLAG = false,
                Default_Value_TXT = null,
                CascadeParentId = null
            };
            ctx.FieldDefinitions.Add(def);
            await ctx.SaveChangesAsync();
        }

        //3) migrate to latest and validate
        await using (var ctx = new SqlServerMetadataDbContext(_options))
        {
            await ctx.Database.MigrateAsync();
            var versions = await ctx.MetadataVersions.CountAsync();
            Assert.True(versions >= 1);
        }
    }
}
