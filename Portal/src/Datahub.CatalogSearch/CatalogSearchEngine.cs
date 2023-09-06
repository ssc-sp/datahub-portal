using Datahub.CatalogSearch.Lucene;
using Datahub.Metadata.Model;
using Microsoft.EntityFrameworkCore;
namespace Datahub.CatalogSearch;

public class CatalogSearchEngine : ICatalogSearchEngine
{
    readonly object _lock;
    readonly IDbContextFactory<MetadataDbContext> _contextFactory;

    private ILanguageCatalogSearch? _englishMetadataSearchEngine;
    private ILanguageCatalogSearch? _frenchMetadataSearchEngine;

    private ILanguageCatalogSearch? _englishDatahubSearchEngine;
    private ILanguageCatalogSearch? _frenchDatahubSearchEngine;

    public CatalogSearchEngine(IDbContextFactory<MetadataDbContext> contextFactory)
    {
        _lock = new object();
        _contextFactory = contextFactory;
    }

    public ILanguageCatalogSearch GetMetadataEnglishSearchEngine()
    {
        lock (_lock)
        {
            _englishMetadataSearchEngine ??= GetLanguageCatalogSearch("en", GetEnglishDocuments);
            return _englishMetadataSearchEngine;
        }
    }

    public ILanguageCatalogSearch GetMetadataFrenchSearchEngine()
    {
        lock (_lock)
        {
            _frenchMetadataSearchEngine ??= GetLanguageCatalogSearch("fr", GetFrenchDocuments);
            return _frenchMetadataSearchEngine!;
        }
    }

    public ILanguageCatalogSearch GetDatahubEnglishSearchEngine(Func<IEnumerable<CatalogDocument>> englishDataReader)
    {
        lock (_lock)
        {
            _englishDatahubSearchEngine ??= GetLanguageCatalogSearch("en", englishDataReader);
            return _englishDatahubSearchEngine!;
        }
    }

    public ILanguageCatalogSearch GetDatahubFrenchSearchEngine(Func<IEnumerable<CatalogDocument>> frenchDataReader)
    {
        lock (_lock)
        {
            _frenchDatahubSearchEngine ??= GetLanguageCatalogSearch("fr", frenchDataReader);
            return _frenchDatahubSearchEngine!;
        }
    }

    private ILanguageCatalogSearch GetLanguageCatalogSearch(string language, Func<IEnumerable<CatalogDocument>> dataReader)
    {
        var catalogSearch = new LanguageCatalogSearch(language);
        foreach (var document in dataReader.Invoke())
        {
            var lowerTitle = (document.Title ?? "").ToLower();
            var lowerContent = (document.Content ?? "").ToLower();
            if (!string.IsNullOrWhiteSpace(lowerTitle) || !string.IsNullOrWhiteSpace(lowerContent))
            {
                catalogSearch.AddDocument(document.Id, lowerTitle, lowerContent);
            }
        }
        catalogSearch.FlushIndexes();
        return catalogSearch;
    }

    private IEnumerable<CatalogDocument> GetEnglishDocuments()
    {
        using var ctx = _contextFactory.CreateDbContext();
        return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Name_TXT, e.Search_English_TXT));
    }

    private IEnumerable<CatalogDocument> GetFrenchDocuments()
    {
        using var ctx = _contextFactory.CreateDbContext();
        return ctx.CatalogObjects.Select(e => new CatalogDocument(e.CatalogObjectId.ToString(), e.Name_French_TXT, e.Search_French_TXT));
    }
}

public record CatalogDocument(string Id, string Title, string Content);
