using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;

// todo: read from args
var path = @"..\..\..\..\Portal\src\Datahub.Portal\i18n\ssc";

var files = Directory.GetFiles(path);
foreach (var file in files)
{
    var fileName = Path.GetFileName(file);
    if (fileName.Contains(".fr.", StringComparison.OrdinalIgnoreCase))
        continue;
    
    List<LocalizationEntry> rows = new();
    var dict = ParseDictionary(File.ReadAllText(file));
    Traverse(dict, default, (k, v) => rows.Add(new(k, v, "")));

    var fileExt = Path.GetExtension(file);
    var outputName = Path.ChangeExtension(Path.GetFileName(file), $".csv");
    var outputPath = $"./{outputName}";

    OutputFile(outputPath, rows);
}

static void OutputFile(string path, List<LocalizationEntry> rows)
{
    using var writer = new StreamWriter(path);
    using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
    csv.WriteRecords(rows);
}

static IDictionary<string, object> ParseDictionary(string content)
{
    return JsonConvert.DeserializeObject<Dictionary<string, object>>(content) ?? new();
}

static string MakePath(string? path, string name) => path is null ? name : $"{path}/{name}";

static void Traverse(IDictionary<string, object> dictionary, string? path, Action<string, string> addPair)
{
    foreach (var kv in dictionary)
    {
        if (kv.Value is String str)
        {
            var trimmed = str.Trim();

            if (string.IsNullOrEmpty(trimmed))
                continue;

            addPair(MakePath(path, kv.Key), trimmed);
        }
        else
        {
            var valueAsStr = JsonConvert.SerializeObject(kv.Value);
            var valueDictionary = ParseDictionary(valueAsStr);
            Traverse(valueDictionary, MakePath(path, kv.Key), addPair);
        }
    }
}

record LocalizationEntry(string Key, string English, string French);


