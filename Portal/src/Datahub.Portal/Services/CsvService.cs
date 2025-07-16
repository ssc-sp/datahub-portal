using CsvHelper;
using Datahub.Core.Services;
using static Datahub.Core.Services.ICsvService;
using System.Globalization;

namespace Datahub.Portal.Services;

public class CsvService : ICsvService
{
    private static StreamWriter InitStreamWriter(Stream stream) => new(stream, System.Text.Encoding.UTF8);
    private static CsvWriter InitCsvWriter(TextWriter writer) => new(writer, CultureInfo.InvariantCulture);

    public Stream GenerateCsvStreamFromDynamicRecords(IEnumerable<dynamic> dynamicRecords)
    {
        var stream = new MemoryStream();
        var writer = InitStreamWriter(stream);
        var csvWriter = InitCsvWriter(writer);

        if (dynamicRecords != null && dynamicRecords.Any())
        {
            var first = dynamicRecords.First();
            csvWriter.WriteDynamicHeader(first);
            csvWriter.NextRecord();

            foreach (var dynamicRecord in dynamicRecords)
            {
                csvWriter.WriteRecord(dynamicRecord);
                csvWriter.NextRecord();
            }
        }

        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    public Stream GenerateCsvStreamFromRecords<TData>(IEnumerable<TData> records)
        where TData : ICsvRecord
    {
        var stream = new MemoryStream();
        var writer = InitStreamWriter(stream);
        var csvWriter = InitCsvWriter(writer);

        csvWriter.WriteRecords(records);

        writer.Flush();
        stream.Position = 0;
        return stream;
    }
}
