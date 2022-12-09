using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datahub.Metadata.Utils;

public static class FieldValueContainerExtensions
{
    public static CatalogDigest GetCatalogDigest(this FieldValueContainer fields)
    {
        var objectName = fields.GetValue("name");
        var contact = fields.GetValue("contact_information");

        var sectorChoice = fields.GetSelectedChoices("sector").FirstOrDefault();
        var branchChoice = fields.GetSelectedChoices("branch").FirstOrDefault();

        _ = int.TryParse(sectorChoice?.Value_TXT ?? "0", out int sector);
        _ = int.TryParse(branchChoice?.Value_TXT ?? "0", out int branch);

        var subjects = fields.GetSelectedChoices("subject").ToList();
        var programs = fields.GetSelectedChoices("programs").ToList();

        var englishKeywords = fields.GetValue("keywords_en").Split(',').ToList();
        var frenchKeywords = fields.GetValue("keywords_fr").Split(',').ToList();

        var englishCatalog = GetCatalogText(subjects.Select(s => s.Label_English_TXT), programs.Select(p => p.Value_TXT),
            sectorChoice?.Label_English_TXT, branchChoice?.Label_English_TXT, objectName, englishKeywords);

        var frenchCatalog = GetCatalogText(subjects.Select(s => s.Label_French_TXT), programs.Select(p => p.Value_TXT),
            sectorChoice?.Label_French_TXT, branchChoice?.Label_French_TXT, objectName, frenchKeywords);

        var titleEnglish = fields.GetValue("title_translated_en", objectName);
        var titleFrench = fields.GetValue("title_translated_fr", objectName);

        return new(titleEnglish, titleFrench, contact, sector, branch, englishCatalog, frenchCatalog, 
            fields.GetSecurityClassification());
    }

    const string SecurityClassificationName = "security_classification";

    public static ClassificationType GetSecurityClassification(this FieldValueContainer fields)
    {
        return fields.GetValue(SecurityClassificationName, "0") switch
        {
            "1" => ClassificationType.ProtectedA,
            "2" => ClassificationType.ProtectedB,
            _   => ClassificationType.Unclassified
        };
    }

    public static void EnforceSecurityClassification(this FieldValueContainer fields)
    {
        if (string.IsNullOrEmpty(fields.GetValue(SecurityClassificationName)))
            fields.SetValue(SecurityClassificationName, "0");
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
            .ToLower();
    }
}