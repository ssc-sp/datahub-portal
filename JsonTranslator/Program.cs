using Datahub.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

var src = @"..\..\..\..\WebPortal\i18n\localizations2.json";
var fPath = Path.GetFullPath(src);
if (!File.Exists(fPath))
{
    throw new Exception($"File {fPath} does not exist");
}
var tgt = Path.Combine(Path.GetDirectoryName(fPath)!, "output-fr.json");
using var sourceFile = File.OpenRead(src);
var data = await JsonSerializer.DeserializeAsync<Dictionary<string,string>>(sourceFile);
var cfgPath = Path.GetFullPath(@"../../../../WebPortal");
var config = new ConfigurationBuilder()
    //.SetBasePath(AppContext.BaseDirectory)
    .SetBasePath(cfgPath)
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile("appsettings.Development.json", false, true)
    .Build();

var translator = new TranslationService(config);
var newDic = new Dictionary<string, string>();
foreach (var item in data.Where(d => !String.IsNullOrWhiteSpace(d.Key)))
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
