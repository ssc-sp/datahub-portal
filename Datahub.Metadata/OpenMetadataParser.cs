using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Datahub.Metadata
{
    public class OpenMetadataParser
    {
        public static MetadataDefinition Parse(string csvData, bool ignoreDuplicateDefinitions)
        {
            var definitions = new MetadataDefinition(ignoreDuplicateDefinitions);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = false };
            using var reader = new StringReader(csvData);
            using var csvReader = new CsvReader(reader, config);

            // skip the header
            csvReader.Read();

            var sortOrder = 1;
            foreach (var row in csvReader.GetRecords<OpenMetadataField>())
            {
                var fieldDefinition = new FieldDefinition()
                {
                    Id = row.FieldId,
                    NameEnglish = row.NameEnglish,
                    NameFrench = row.NameFrench,
                    DescriptionEnglish = row.DescriptionEnglish,
                    DescriptionFrench = row.DescriptionFrench,
                    ObligationEnglish = row.ObligationEnglish,
                    ObligationFrench = row.ObligationFrench,
                    SortOrder = sortOrder++
                };
                definitions.Add(fieldDefinition);
            }

            return definitions;
        }
    }
}
