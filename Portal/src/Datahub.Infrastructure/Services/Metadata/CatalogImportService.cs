using System.Text;
using Datahub.Application.Services.UserManagement;
using Datahub.Core.Data;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.Metadata;
using Datahub.Metadata.Catalog;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Datahub.Infrastructure.Services.Metadata;

public class CatalogImportService
{
    private readonly ILogger<CatalogImportService> _logger;
    private readonly IMetadataBrokerService _metadataBrokerService;
    private readonly IDbContextFactory<MetadataDbContext> _metadataCtxFactory;
    private readonly IDbContextFactory<DatahubProjectDBContext> _datahubProjectDbFactory;
    private readonly IMSGraphService _msGraphService;
    private readonly Dictionary<string, GraphUser> _userCache = new();

    public CatalogImportService(ILogger<CatalogImportService> logger,
        IDbContextFactory<MetadataDbContext> metadataCtxFactory,
        IMetadataBrokerService metadataBrokerService,
        IDbContextFactory<DatahubProjectDBContext> datahubProjectDbFactory,
        IMSGraphService msGraphService)
    {
        _logger = logger;
        _metadataCtxFactory = metadataCtxFactory;
        _metadataBrokerService = metadataBrokerService;
        _datahubProjectDbFactory = datahubProjectDbFactory;
        _msGraphService = msGraphService;
    }

    public async Task<bool> Import(Stream content)
    {
        try
        {
            var reader = new StreamReader(content);
            var json = await reader.ReadToEndAsync();
            var entries = JsonConvert.DeserializeObject<CatalogEntry[]>(json).ToList();

            var metadataCtx = _metadataCtxFactory.CreateDbContext();
            var projectCtx = _datahubProjectDbFactory.CreateDbContext();

            foreach (var entry in entries)
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

                var graphUser = await GetGraphUser(entry.contact);
                var sector = await GetSector(projectCtx, graphUser?.Department);

                // save dataset metadata
                var fieldValues = await _metadataBrokerService.GetObjectMetadataValues(objMetadata.ObjectMetadataId);
                var subjectValues = GetSubjectValues(fieldValues.Definitions, entry.subjects) ?? "";

                fieldValues.SetValue("sector", $"{sector?.Id ?? 0}");
                fieldValues.SetValue("collection", "primary");

                fieldValues.SetValue("title_translated_en", entry.nameEn ?? string.Empty);
                fieldValues.SetValue("title_translated_fr", entry.nameFr ?? string.Empty);

                fieldValues.SetValue("notes_translated_en", entry.descEn ?? string.Empty);
                fieldValues.SetValue("notes_translated_fr", entry.descFr ?? string.Empty);

                fieldValues.SetValue("contact_information", graphUser?.Mail ?? entry.contact);
                fieldValues.SetValue("subject", subjectValues);

                fieldValues.SetValue("keywords_en", string.Join(",", entry.keywordsEn));
                fieldValues.SetValue("keywords_fr", string.Join(",", entry.keywordsFr));

                await _metadataBrokerService.SaveMetadata(fieldValues, true);

                var catalogObj = new CatalogObject()
                {
                    ObjectMetadataId = objMetadata.ObjectMetadataId,
                    DataType = MetadataObjectType.DatasetUrl,
                    Name_TXT = entry.nameEn,
                    Name_French_TXT = entry.nameFr,
                    Url_English_TXT = entry.urlEn,
                    Url_French_TXT = entry.urlFr,
                    SecurityClass_TXT = entry.classification ?? "Unclassified",
                    Classification_Type = GetClassificationType(entry.classification),
                    Sector_NUM = sector?.Id ?? 0,
                    Branch_NUM = 0, // no branch for now
                    Contact_TXT = graphUser?.Mail ?? entry.contact,
                    Search_English_TXT = GetCatalogText(GetSubjects(entry, true), GetPrograms(entry), sector?.NameEnglish ?? "", string.Empty, entry.nameEn, entry.keywordsEn),
                    Search_French_TXT = GetCatalogText(GetSubjects(entry, false), GetPrograms(entry), sector?.NameFrench ?? "", string.Empty, entry.nameFr, entry.keywordsFr),
                };

                metadataCtx.CatalogObjects.Add(catalogObj);
                metadataCtx.SaveChanges();
            }

            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError("Cannot import catalog file!", ex);
            return false;
        }
    }

    private async Task<GraphUser> GetGraphUser(string userName)
    {
        if (_userCache.ContainsKey(userName))
        {
            return _userCache[userName];
        }

        var graphUser = await _msGraphService.GetUserFromSamAccountNameAsync(userName, default);
        if (graphUser is not null)
        {
            _userCache[userName] = graphUser;
        }

        return graphUser;
    }

    static bool ObjectMetadataExists(MetadataDbContext ctx, string id)
    {
        return ctx.ObjectMetadataSet.Any(e => e.ObjectId_TXT == id);
    }

    static async Task<CatalogEntrySector> GetSector(DatahubProjectDBContext ctx, string department)
    {
        var engAcronym = (department ?? "").Split('.').FirstOrDefault();
        if (!string.IsNullOrEmpty(engAcronym))
        {
            var orgLevel = await ctx.OrganizationLevels.FirstOrDefaultAsync(e => e.FullAcronymE == engAcronym);
            if (orgLevel is not null)
            {
                return new()
                {
                    Id = orgLevel.OrganizationID,
                    NameEnglish = orgLevel.OrgNameE,
                    NameFrench = orgLevel.OrgNameF
                };
            }
        }
        return new();
    }

    static string GetSubjectValues(FieldDefinitions definitions, List<CatalogEntrySubject> subjects)
    {
        var subjectField = definitions.Get("subject");

        var choices = subjectField?.Choices.ToList();
        if (choices == null)
            return "";

        List<string> values = new();
        foreach (var subject in subjects.Select(s => s.nameEn))
        {
            var found = choices.FirstOrDefault(c => c.Label_English_TXT.Equals(subject, StringComparison.InvariantCultureIgnoreCase));
            if (found != null)
                values.Add(found.Value_TXT);
        }

        return string.Join("|", values);
    }

    static IEnumerable<string> GetSubjects(CatalogEntry entry, bool eng)
    {
        return entry.subjects is not null ? entry.subjects.Select(s => eng ? s.nameEn : s.nameFr) : new List<string>();
    }

    static IEnumerable<string> GetPrograms(CatalogEntry entry)
    {
        return (entry.programs ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s));
    }

    static string GetCatalogText(IEnumerable<string> subjects, IEnumerable<string> programs, string sector, string branch, string objectName, IEnumerable<string> keywords)
    {
        //return new StringBuilder()
        //  .AppendJoin(' ', keywords)
        //  .Append(' ')
        //  .AppendJoin(' ', subjects)
        //  .Append(' ')
        //  .AppendJoin(' ', programs.Where(p => p != "none"))
        //  .Append(' ')
        //  .Append($"{sector} {branch} {objectName}")
        //  .ToString()
        //  .ToLower()
        //  .Trim();
        return new StringBuilder()
            .AppendJoin(' ', keywords)
            .Append(' ')
            .AppendJoin(' ', programs.Where(p => p != "none"))
            .Append(' ')
            .Append(objectName)
            .ToString()
            .ToLower()
            .Trim();
    }

    static ClassificationType GetClassificationType(string value)
    {
        var normalized = string.Join("", (value ?? "").Split(' ')).ToLower();
        return normalized switch
        {
            "protecteda" => ClassificationType.ProtectedA,
            "protectedb" => ClassificationType.ProtectedB,
            _ => ClassificationType.Unclassified
        };
    }
}