using DeepL;
using Microsoft.Extensions.Configuration;

namespace Datahub.Core.Services;

public class TranslationService
{
    private readonly Translator _client;

    public TranslationService(IConfiguration configuration)
    {
        var useFreeApi = configuration.GetSection("DeepL").GetValue("UseFreeApi", true);
        var authKey = configuration.GetSection("DeepL").GetValue<string>("AuthKey");
        _client = new Translator(authKey, new TranslatorOptions() { ServerUrl = useFreeApi ? "https://api-free.deepl.com" : null });
    }

    public async Task<string> GetFrenchTranslation(string englishText)
    {
        return await TranslateTo(englishText, LanguageCode.English, LanguageCode.French);
    }

    public async Task<string> GetEnglishTranslation(string frenchText)
    {
        return await TranslateTo(frenchText, LanguageCode.French, LanguageCode.English);
    }

    public async Task<string> TranslateTo(string text, string sourceLanguage, string targetLanguage)
    {
        var translation = await _client.TranslateTextAsync(text, sourceLanguage, targetLanguage);
        return translation?.Text ?? String.Empty;
    }
}