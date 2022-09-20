using Datahub.CatalogSearch.Lucene;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;

namespace Datahub.CatalogSearch
{
    public class CatalogSearchEngine : ICatalogSearchEngine
    {
        readonly object _lock;
        readonly IDbContextFactory<MetadataDbContext> _contextFactory;

        private ILanguageCatalogSearch? _englishSearchEngine;
        private ILanguageCatalogSearch? _frenchSearchEngine;

        public CatalogSearchEngine(IDbContextFactory<MetadataDbContext> contextFactory)
        {
            _lock = new object();
            _contextFactory = contextFactory;
        }

        public ILanguageCatalogSearch GetEnglishSearchEngine()
        {
            lock (_lock)
            {
                _englishSearchEngine ??= GetLanguageCatalogSearch("en", GetEnglishDocuments);
                return _englishSearchEngine;
            }
        }

        public ILanguageCatalogSearch GetFrenchSearchEngine()
        {
            lock (_lock)
            {
                _frenchSearchEngine ??= GetLanguageCatalogSearch("fr", GetFrenchDocuments);
                return _frenchSearchEngine!;
            }
        }

        private ILanguageCatalogSearch GetLanguageCatalogSearch(string language, Func<MetadataDbContext, IEnumerable<CatalogDocument>> dataReader)
        {
            using var ctx = _contextFactory.CreateDbContext();
            var catalogSearch = new LanguageCatalogSearch(language);
            foreach (var document in dataReader(ctx))
            {
                var lowerContent = (document.Content ?? "").ToLower();
                if (!string.IsNullOrWhiteSpace(lowerContent))
                {
                    catalogSearch.AddDocument(document.Id, lowerContent);
                }
            }
            catalogSearch.FlushIndexes();
            return catalogSearch;
        }

        private IEnumerable<CatalogDocument> GetEnglishDocuments(MetadataDbContext ctx)
        {
            //return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Search_English_TXT));
            return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Name_TXT));
        }

        private IEnumerable<CatalogDocument> GetFrenchDocuments(MetadataDbContext ctx)
        {
            //return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Search_French_TXT));
            return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Name_French_TXT));
        }
    }

    record CatalogDocument(string Id, string Content);    
}
