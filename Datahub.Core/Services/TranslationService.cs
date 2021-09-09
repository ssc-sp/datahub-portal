using DeepL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRCan.Datahub.Shared.Services
{
    public class TranslationService
    {
        DeepLClient client = new DeepLClient("44a39371-3099-1bdf-f50a-2410747fbfd5", useFreeApi: true);

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
