using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.CatalogSearch
{
    public class CatalogSearchEngine : ICatalogSearchEngine
    {
        public ILanguageCatalogSearch GetEnglishSearchEngine()
        {
            throw new NotImplementedException();
        }

        public ILanguageCatalogSearch GetFrenchSearchEngine()
        {
            throw new NotImplementedException();
        }
    }
}
