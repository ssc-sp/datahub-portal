using DeepL;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class TranslationService
    {
        private readonly DeepLClient _client;

        public TranslationService(IConfiguration configuration)
        {
            var useFreeApi = configuration.GetSection("DeepL").GetValue<bool>("UseFreeApi");
            var authKey = configuration.GetSection("DeepL").GetValue<string>("AuthKey");
            _client = new DeepLClient(authKey, useFreeApi);
        }

        public async Task<string> GetFrenchTranslation(string englishText)
        {
            return await TranslateTo(englishText, Language.English, Language.French);
        }

        public async Task<string> GetEnglishTranslation(string frenchText)
        {
            return await TranslateTo(frenchText, Language.French, Language.English);
        }

        public async Task<string> TranslateTo(string text, Language sourceLanguage, Language targetLanguage)
        {
            var translation = await _client.TranslateAsync(text, sourceLanguage, targetLanguage);
            return translation?.Text ?? String.Empty;
        }
    }
}
