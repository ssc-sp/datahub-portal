// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using System.Text;
using Azure.Identity;
using CatalogIngestTool;
using Datahub.Application.Services;
using Datahub.Application.Services.Metadata;
using Datahub.Core.Model.Context;
using Datahub.Core.Model.Datahub;
using Datahub.Infrastructure.Services.Metadata;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using GraphServiceClient = Microsoft.Graph.GraphServiceClient;

#region Config

var config = new ConfigurationBuilder()
     .SetBasePath(System.IO.Directory.GetCurrentDirectory())
     .AddJsonFile($"appsettings.Development.json")
     .Build();

string projectDbConnectionString  = config[nameof(projectDbConnectionString)]!;
string metadataDbConnectionString = config[nameof(metadataDbConnectionString)]!;
string tenantId = config["TenantId"]!;
string clientId = config["ClientId"]!;
string clientSecret = config["ClientSecret"]!;

#endregion

#region Services and Database config

var services = new ServiceCollection();
services.AddDbContext<MetadataDbContext>(options => options.UseSqlServer(metadataDbConnectionString));
services.AddPooledDbContextFactory<MetadataDbContext>(options => options.UseSqlServer(metadataDbConnectionString));
services.AddDbContext<DatahubProjectDBContext>(options => options.UseSqlServer(projectDbConnectionString));

services.AddLogging(configure => configure.AddConsole());

services.AddScoped<IDatahubAuditingService, DummyDatahubAuditingService>();
services.AddScoped<IMetadataBrokerService, MetadataBrokerService>();

var serviceProvider = services.BuildServiceProvider();

var graphClient = PrepareAuthenticatedClient(tenantId, clientId, clientSecret);

var metadataCtx = serviceProvider.GetService<MetadataDbContext>();
var projectCtx = serviceProvider.GetService<DatahubProjectDBContext>();

var metadataBroker = serviceProvider.GetService<IMetadataBrokerService>();

#endregion

if (metadataCtx is not null && projectCtx is not null && InputFileExists(args))
{
    Dictionary<string, User> userCache = new();

    Console.WriteLine($"Loading file '{args[0]}'...");

    // Load json data
    var json = File.ReadAllText(args[0]);
    Entry[] entries = JsonConvert.DeserializeObject<Entry[]>(json) ?? Array.Empty<Entry>();

    Console.WriteLine($"Processing file {entries?.Length} rows..");

    var sw = Stopwatch.StartNew();

    foreach (var entry in entries!)
    {
        // check there is no ObjectMetadata entry already (continue if so..)
        if (ObjectMetadataExists(metadataCtx, entry.id))
            continue;

        // create ObjectMetadata
        ObjectMetadata objMetadata = new()
        {
            ObjectId_TXT = entry.id,
            MetadataVersionId = 2
        };

        // save metadata object entry
        metadataCtx.ObjectMetadataSet.Add(objMetadata);
        metadataCtx.SaveChanges();

        var graphUser = await GetGraphUser(graphClient, entry.contact, userCache);
        var sector = await GetSector(projectCtx, graphUser?.Department);

        // save dataset metadata
        var fieldValues = await metadataBroker!.GetObjectMetadataValues(objMetadata.ObjectMetadataId);
        var subjectValues = GetSubjectValues(fieldValues.Definitions, entry.subjects) ?? "";

        fieldValues.SetValue("sector", $"{sector?.Id ?? 0}");
        fieldValues.SetValue("collection", "primary");
        fieldValues.SetValue("title_translated_en", entry.name_en);
        fieldValues.SetValue("title_translated_fr", entry.name_fr);
        fieldValues.SetValue("contact_information", graphUser?.Mail ?? entry.contact);
        fieldValues.SetValue("subject", subjectValues);
        fieldValues.SetValue("keywords_en", String.Join(",", entry.keywords_en));
        fieldValues.SetValue("keywords_fr", String.Join(",", entry.keywords_fr));
        await metadataBroker.SaveMetadata(fieldValues, true);

        var catalogObj = new CatalogObject()
        {
            ObjectMetadataId = objMetadata.ObjectMetadataId,
            DataType = MetadataObjectType.DatasetUrl,
            Name_TXT = entry.name_en,
            Name_French_TXT = entry.name_fr,
            Location_TXT = entry.url_en,
            SecurityClass_TXT = entry.securityClass ?? "Unclassified",
            Sector_NUM = sector?.Id ?? 0,
            Branch_NUM = 0, // no branch for now
            Contact_TXT = graphUser?.Mail ?? entry.contact,
            Search_English_TXT = GetCatalogText(GetSubjects(entry, true), GetPrograms(entry), 
                sector?.Name_English ?? "", string.Empty, entry.name_en, entry.keywords_en),
            Search_French_TXT = GetCatalogText(GetSubjects(entry, false), GetPrograms(entry), 
                sector?.Name_French ?? "", string.Empty, entry.name_fr, entry.keywords_fr),
        };

        metadataCtx.CatalogObjects.Add(catalogObj);
        metadataCtx.SaveChanges();
    }

    sw.Stop();

    Console.WriteLine($"Processing time: {sw.Elapsed}");
}

Console.WriteLine($"Done...");

Console.ReadLine();

#region utility functions

static bool InputFileExists(string[] args) => args.Length > 0 && File.Exists(args[0]);

static bool ObjectMetadataExists(MetadataDbContext ctx, string id)
{
    return ctx.ObjectMetadataSet.Any(e => e.ObjectId_TXT == id);
}

static Microsoft.Graph.GraphServiceClient PrepareAuthenticatedClient(string tenantId, string clientId, string clientSecret)
{

    //see https://learn.microsoft.com/en-us/graph/sdks/choose-authentication-providers?tabs=csharp
    // using Azure.Identity;
    var options = new ClientSecretCredentialOptions
    {
        AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
    };
    var clientCertCredential = new ClientSecretCredential(
        tenantId,
        clientId,
        clientSecret, options);

    GraphServiceClient graphServiceClient = new(clientCertCredential);

    return graphServiceClient;
}

static async Task<User?> GetGraphUser(GraphServiceClient graphClient, string userName, Dictionary<string, Microsoft.Graph.Models.User> userCache)
{
    if (userCache.ContainsKey(userName))
        return userCache[userName];

    var users = await graphClient.Users.GetAsync(
        requestConfiguration =>
        {
            requestConfiguration.QueryParameters.Search = $"\"onPremisesSamAccountName:{userName}\"";
            requestConfiguration.QueryParameters.Select = new[] { "mail","department"};
            requestConfiguration.QueryParameters.Count = true;
            requestConfiguration.Headers.Add("ConsistencyLevel", "eventual");
        });

    var found = users?.Value?.FirstOrDefault();
    if (found is not null)
        userCache[userName] = found!;

    return found;
}

static async Task<Sector> GetSector(DatahubProjectDBContext ctx, string? department)
{
    var engAcronym = (department ?? "").Split('.').FirstOrDefault();
    if (!string.IsNullOrEmpty(engAcronym))
    {
        var orgLevel = await ctx.Organization_Levels.FirstOrDefaultAsync(e => e.Full_Acronym_E == engAcronym);
        if (orgLevel is not null)
        {
            return new Sector()
            {
                Id = orgLevel.Organization_ID,
                Name_English = orgLevel.Org_Name_E,
                Name_French = orgLevel.Org_Name_F
            };
        }
    }
    return new Sector();
}

static IEnumerable<string> GetSubjects(Entry entry, bool eng)
{
    return entry.subjects is not null ? entry.subjects.Select(s => eng ? s.name_en : s.name_fr) : new List<string>();
}

static IEnumerable<string> GetPrograms(Entry entry)
{
    return (entry.programs ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
}

static string GetCatalogText(IEnumerable<string> subjects, IEnumerable<string> programs, string sector, string branch, string objectName, IEnumerable<string> keywords)
{
    return new StringBuilder()
      .AppendJoin(' ', keywords)
      .Append(' ')
      .AppendJoin(' ', subjects)
      .Append(' ')
      .AppendJoin(' ', programs.Where(p => p != "none"))
      .Append(' ')
      .Append($"{sector} {branch} {objectName}")
      .ToString()
      .ToLower()
      .Trim();
}

string GetSubjectValues(FieldDefinitions definitions, List<CatalogIngestTool.Subject> subjects)
{
    var subjectField = definitions.Get("subject");

    var choices = subjectField?.Choices.ToList();
    if (choices == null)
        return "";

    List<string> values = new();
    foreach (var subject in subjects.Select(s => s.name_en))
    {
        var found = choices.FirstOrDefault(c => c.Label_English_TXT.Equals(subject, StringComparison.InvariantCultureIgnoreCase));
        if (found != null)
            values.Add(found.Value_TXT);
    }

    return String.Join("|", values);
}


#endregion

namespace CatalogIngestTool
{
	class DummyDatahubAuditingService : IDatahubAuditingService
    {
        public Task TrackAdminEvent(string scope, string source, AuditChangeType changeType, params (string Key, string Value)[] details)
        {
            return Task.CompletedTask;
        }

        public Task TrackDataEvent(string objectId, string table, AuditChangeType changeType, bool anonymous, params (string Key, string Value)[] details)
		{
            return Task.CompletedTask;
        }

        public Task TrackException(Exception exception, params (string Key, string Value)[] details)
		{
            return Task.CompletedTask;
        }

        public Task TrackSecurityEvent(string scope, string table, AuditChangeType changeType, params (string Key, string Value)[] details)
		{
            return Task.CompletedTask;
        }

        public Task TrackEvent(string message, params (string Key, string Value)[] details)
		{
            return Task.CompletedTask;
        }
    }
}