
using System.Text.Json;
using System.Text.RegularExpressions;

Console.WriteLine("Hello, World!");

// load in the i18n english file from /src/Datahub.Portal/i18n/localization.json

var englishTranslationText = File.ReadAllText(@"../../../../../src/Datahub.Portal/i18n/localization.json");
var englishTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(englishTranslationText);

var frenchTranslationText = File.ReadAllText(@"../../../../../src/Datahub.Portal/i18n/localization.fr.json");
var frenchTranslations = JsonSerializer.Deserialize<Dictionary<string, string>>(frenchTranslationText);

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

var missingEnglishTranslations = new List<string>();
var missingFrenchTranslations = new List<string>();
foreach (var str in strings)
{
    if (!englishTranslations!.ContainsKey(str))
    {
        missingEnglishTranslations.Add(str);
    }
    
    if (!frenchTranslations!.ContainsKey(str))
    {
        missingFrenchTranslations.Add(str);
    }
}

// output the missing translations to a file
var serializeOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

var englishOutput = missingEnglishTranslations
    .Distinct()
    .ToDictionary(x => x, x => x);
var englishOutputJson = JsonSerializer.Serialize(englishOutput, serializeOptions);

var frenchOutput = missingFrenchTranslations
    .Distinct()
    .ToDictionary(x => x, x => x);
var frenchOutputJson = JsonSerializer.Serialize(frenchOutput, serializeOptions);


File.WriteAllText("missing-translations.en.json", englishOutputJson);
File.WriteAllText("missing-translations.fr.json", frenchOutputJson);

Console.WriteLine("Done!");



