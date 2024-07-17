using Datahub.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using System.Text.Json;

var src = @"../../../../../src/Datahub.Portal/MissingTranslations.json";
var fPath = Path.GetFullPath(src);
if (!File.Exists(fPath))
{
    throw new Exception($"File {fPath} does not exist");
}
var tgt = Path.Combine(Path.GetDirectoryName(fPath)!, "output-fr.json");
using var sourceFile = File.OpenRead(src);
var data = (await JsonSerializer.DeserializeAsync<RootObject>(sourceFile)) ?? new();
var cfgPath = Path.GetFullPath(@"../../../../../src/Datahub.Portal");
var config = new ConfigurationBuilder()
    //.SetBasePath(AppContext.BaseDirectory)
    .SetBasePath(cfgPath)
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", true, false)
    .AddJsonFile("appsettings.sand.json", true, false)
    .Build();
var authKey = config.GetSection("DeepL").GetValue<string>("AuthKey");
if (authKey is null)
{
    config.GetSection("DeepL")["AuthKey"] = Environment.GetEnvironmentVariable("DEEPL_KEY");
}
var translator = new TranslationService(config);
var newDic = new Dictionary<string, string>();
foreach (var item in data.@default.Where(d => !String.IsNullOrWhiteSpace(d.Key)))
{
    if (!string.IsNullOrWhiteSpace(item.Value))
    {
        var translated = await translator.GetFrenchTranslation(item.Value);
        Console.WriteLine($"Translated {item.Value} to {translated}");
        newDic[item.Key] = translated;
    }
    else
    {
        var translated = await translator.GetFrenchTranslation(item.Key);
        Console.WriteLine($"Translated {item.Key} to {translated}");
        newDic[item.Key] = translated;
    }
}
using var outFile = File.OpenWrite(tgt);
var options = new JsonSerializerOptions
{
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    //JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
    WriteIndented = true
};
await JsonSerializer.SerializeAsync(outFile, newDic, options);

public class RootObject
{
    public Dictionary<string, string> @default { get; set; } = new();
}