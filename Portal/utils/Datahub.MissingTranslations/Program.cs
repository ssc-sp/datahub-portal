
using System.Text.Json;
using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");



// load in the i18n english file from /src/Datahub.Portal/i18n/localization.json

var i18n = File.ReadAllText(@"../../../../../src/Datahub.Portal/i18n/localization.json");
var i18nData = JsonSerializer.Deserialize<Dictionary<string, string>>(i18n);

// scan the /src/Datahub.Portal directory for all .cs and .razor files
// for each file, scan the contents for any string after "@Localizer[" or "Localizer["

var files = Directory.GetFiles(@"../../../../../src/Datahub.Portal", "*.cs", SearchOption.AllDirectories)
    .Concat(Directory.GetFiles(@"../../../../../src/Datahub.Portal", "*.razor", SearchOption.AllDirectories));

var strings = new List<string>();
foreach (var file in files)
{
    var content = File.ReadAllText(file);
    var matches = Regex.Matches(content, @"(?:@Localizer\[""|Localizer\["")(.+?)(?:""|\])");
    foreach (Match match in matches)
    {
        strings.Add(match.Groups[1].Value);
    }
}

// for each string, check if it exists in the i18n file
// if it doesn't, add it to a list of missing translations

var missingTranslations = new List<string>();
foreach (var str in strings)
{
    if (!i18nData.ContainsKey(str))
    {
        missingTranslations.Add(str);
    }
}

// output the missing translations to a file

var output = JsonSerializer.Serialize(missingTranslations);
File.WriteAllText("missing-translations.json", output);
Console.WriteLine("Done!");



