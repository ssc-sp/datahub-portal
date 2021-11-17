using Datahub.Core.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

var src = @"C:\code\datahub-portal\ps-translator\data.json";
var tgt = Path.Combine(Path.GetDirectoryName(src), "output-fr.json");
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
foreach (var item in data)
{
    if (!string.IsNullOrWhiteSpace(item.Value))
    {
        var translated = await translator.GetFrenchTranslation(item.Value);
        Console.WriteLine($"Translated {item.Value} to {translated}");
        newDic[item.Key] = translated;
    }
    else
    {
        newDic[item.Key] = "????";
    }
}
using var outFile = File.OpenWrite(tgt);
await JsonSerializer.SerializeAsync(outFile, newDic);
