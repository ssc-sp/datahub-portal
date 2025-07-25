using Datahub.Core.Services;
using Datahub.Portal.Services;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using Xunit;

namespace Datahub.Tests;

public class CsvServiceTests
{
    record SampleRecord(int Id, string Name, decimal? Amount) : ICsvService.ICsvRecord;

    private readonly ICsvService _csvService = new CsvService();

    [Fact]
    public void GenerateCsvStreamFromRecords_ShouldProduceValidCsvWithUtf8Bom()
    {
        var records = new List<SampleRecord>
        {
            new(1, "Alice", 10.00m),
            new(2, "Bob", 12.34m)
        };

        using var stream = _csvService.GenerateCsvStreamFromRecords(records);

        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer);

        Assert.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), text);
        Assert.Contains("Id,Name,Amount", text);
        Assert.Contains("1,Alice,10.00", text);
        Assert.Contains("2,Bob,12.34", text);
    }

    [Fact]
    public void GenerateCsvStreamFromDynamicRecords_ShouldProduceValidCsvWithUtf8Bom()
    {
        dynamic dynA = new ExpandoObject();
        dynamic dynB = new ExpandoObject();

        dynA.Id = 10;
        dynA.Name = "Charlie";
        dynA.Amount = 22.22m;

        dynB.Id = 20;
        dynB.Name = "Dana";

        var dynamicRecords = new List<dynamic>
        {
            dynA, dynB
        };

        using var stream = _csvService.GenerateCsvStreamFromDynamicRecords(dynamicRecords);

        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer);

        Assert.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), text);
        Assert.Contains("Id,Name,Amount", text);
        Assert.Contains("10,Charlie,22.22", text);
        Assert.Contains("20,Dana", text);
    }

    [Fact]
    public void GenerateCsvStreamFromRecords_ShouldHandleNullAmount()
    {
        var records = new List<SampleRecord>
        {
            new(1, "Alice", 100.50m),
            new(2, "Bob", null)
        };

        using var stream = _csvService.GenerateCsvStreamFromRecords(records);
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer);

        Assert.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), text);
        Assert.Contains("Id,Name,Amount", text);
        Assert.Contains("1,Alice,100.50", text);
        Assert.Contains("2,Bob,", text); // Null amount should be empty in CSV
    }

    [Fact]
    public void GenerateCsvStreamFromRecords_ShouldHandleEmptyCollection()
    {
        var emptyRecords = new List<SampleRecord>();

        using var stream = _csvService.GenerateCsvStreamFromRecords(emptyRecords);
        var buffer = new byte[stream.Length];
        stream.Read(buffer, 0, buffer.Length);
        var text = Encoding.UTF8.GetString(buffer);

        // Depending on implementation, CSV may contain only headers or be empty
        Assert.StartsWith(Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble()), text);
        Assert.Contains("Id,Name,Amount", text);
    }
}
