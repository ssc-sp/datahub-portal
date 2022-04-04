using Datahub.Metadata.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Datahub.Metadata.Utils
{
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

            return new(objectName, contact, sector, branch, englishCatalog, frenchCatalog);
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
}
