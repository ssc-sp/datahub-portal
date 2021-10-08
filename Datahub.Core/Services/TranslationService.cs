using DeepL;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class TranslationService
    {

        DeepLClient client;

        public TranslationService(IConfiguration configuration)
        {
            var useFreeApi = configuration.GetSection("DeepL").GetValue<bool>("UseFreeApi");
            var authKey = configuration.GetSection("DeepL").GetValue<string>("AuthKey");
            client = new DeepLClient(authKey, useFreeApi);
        }
        public async Task<string> GetFrenchTranslation(string englishText)
        {
            var translation = await client.TranslateAsync(
                        englishText,
                        Language.French
                    );
            return translation.Text;
        }
    }
}
