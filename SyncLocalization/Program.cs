using CsvHelper;
using Newtonsoft.Json;
using System.Globalization;

//var path = @"..\..\..\..\Portal\src\Datahub.Portal\i18n";
if (args.Length == 0)
    return;

var path = args[0];

var files = Directory.GetFiles(path);
foreach (var file in files)
{
    var fileName = Path.GetFileName(file);
    if (fileName.Contains(".fr.", StringComparison.OrdinalIgnoreCase))
        continue;

    var translFile = GetTranslationFileName(file);
    var translations = LoadFileKeys(translFile);

    Func<string, string> translFunc = k => translations.TryGetValue(k, out var t) ? t : "";
    var rows = LoadFile(file, translFunc);

    var outputPath = $"./{GetOutputFile(file)}";
    OutputFile(outputPath, rows);
}

static string GetOutputFile(string fileName) => Path.ChangeExtension(Path.GetFileName(fileName), $".csv");

static string GetTranslationFileName(string path)
{
    var ext = Path.GetExtension(path);
    return Path.ChangeExtension(path, $".fr{ext}");
}

static List<LocalizationEntry> LoadFile(string file, Func<string, string> tranlator)
{
    List<LocalizationEntry> rows = new();
    if (File.Exists(file))
    {
        Traverse(ParseDictionary(File.ReadAllText(file)), default, (k, v) => rows.Add(new(k, v, tranlator(k))));
    }
    return rows;
}

static IDictionary<string, string> LoadFileKeys(string file)
{
    Dictionary<string, string> keys = new();
    if (File.Exists(file))
    {
        Traverse(ParseDictionary(File.ReadAllText(file)), default, (k, v) => keys[k] = v);
    }
    return keys;
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
