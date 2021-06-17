using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace Datahub.Metadata.CSV
{
    public class CsvMetadataParser
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
            foreach (var row in csvReader.GetRecords<CsvMetadataField>())
            {
                var fieldDefinition = new FieldDefinition()
                {
                    Id = row.FieldId,
                    NameEnglish = row.LabelEnglish,
                    NameFrench = row.LabelFrench,
                    DescriptionEnglish = row.DescriptionEnglish,
                    DescriptionFrench = row.DescriptionFrench,
                    SortOrder = sortOrder++
                };
                definitions.Add(fieldDefinition);
            }

            return definitions;
        }
    }
}
