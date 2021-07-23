using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System;
using NRCan.Datahub.Metadata.Model;

namespace NRCan.Datahub.Metadata
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
                    Field_Name_TXT = row.FieldId,
                    Name_English_TXT = row.NameEnglish,
                    Name_French_TXT = row.NameFrench,
                    English_DESC = row.DescriptionEnglish,
                    French_DESC = row.DescriptionFrench,
                    Required_FLAG = "Mandatory".Equals(row.ObligationEnglish, StringComparison.InvariantCultureIgnoreCase),
                    Sort_Order_NUM = sortOrder++
                };
                definitions.Add(fieldDefinition);
            }

            return definitions;
        }
    }
}
