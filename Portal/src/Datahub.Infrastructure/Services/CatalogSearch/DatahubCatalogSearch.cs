using Datahub.CatalogSearch;
using Datahub.Core.Model.Catalog;
using Datahub.Core.Model.Datahub;
using Datahub.Core.Services.CatalogSearch;
using Microsoft.EntityFrameworkCore;

namespace Datahub.Infrastructure.Services.CatalogSearch;

public class DatahubCatalogSearch : IDatahubCatalogSearch
{
    private readonly IDbContextFactory<DatahubProjectDBContext> _contextFactory;
    private readonly ICatalogSearchEngine _catalogSearchEngine;

    public DatahubCatalogSearch(IDbContextFactory<DatahubProjectDBContext> contextFactory, ICatalogSearchEngine catalogSearchEngine)
    {
        _contextFactory = contextFactory;
        _catalogSearchEngine = catalogSearchEngine;
    }

    public async Task<List<CatalogObject>> SearchCatalog(DatahubSearchRequest query)
    {
        var searchEngine = GetSearchEngine(query.French);

        var results = searchEngine
            .SearchDocuments(query.Text, query.MaxResults)
            .Select(int.Parse)
            .ToHashSet();
        if (results.Count == 0)
            return new List<CatalogObject>();

        await using var ctx = await _contextFactory.CreateDbContextAsync();

        var catalogHits = await ctx.CatalogObjects.Where(e => results.Contains(e.Id)).ToListAsync();

        return catalogHits;
    }

    public async Task AddCatalogObject(CatalogObject value)
    {
        var docId = await UpsertCatalogObject(value);

        var engSearchEngine = GetSearchEngine(french: false);
        engSearchEngine.AddDocument(docId.ToString(), value.NameEnglish, value.DescEnglish);
        engSearchEngine.FlushIndexes();

        var frenchSearchEngine = GetSearchEngine(french: true);
        frenchSearchEngine.AddDocument(docId.ToString(), value.NameFrench, value.DescFrench);
        frenchSearchEngine.FlushIndexes();
    }

    private async Task<int> UpsertCatalogObject(CatalogObject value)
    {
        using var ctx = await _contextFactory.CreateDbContextAsync();

        var existing = await ctx.CatalogObjects.FirstOrDefaultAsync(e => e.ObjectType == value.ObjectType && e.ObjectId == value.ObjectId);
        if (existing != null)
        {
            existing.NameEnglish = value.NameEnglish;
            existing.NameFrench = value.NameFrench;
            existing.DescEnglish = value.DescEnglish;
            existing.DescFrench = value.DescFrench;
        }
        else
        {
            ctx.CatalogObjects.Add(value);
        }
        await ctx.SaveChangesAsync();

        return (existing ?? value).Id;
    }

    private ILanguageCatalogSearch GetSearchEngine(bool french)
    {
        return french ? _catalogSearchEngine.GetDatahubFrenchSearchEngine(GetFrenchDocuments)
                      : _catalogSearchEngine.GetDatahubEnglishSearchEngine(GetEnglishDocuments);
    }

    private IEnumerable<CatalogDocument> GetEnglishDocuments()
    {
        using var ctx = _contextFactory.CreateDbContext();
        return ctx.CatalogObjects.Select(GetEnglishDocument).ToList();
    }

    private IEnumerable<CatalogDocument> GetFrenchDocuments()
    {
        using var ctx = _contextFactory.CreateDbContext();
        return ctx.CatalogObjects.Select(GetFrenchDocument).ToList();
    }

    static CatalogDocument GetEnglishDocument(CatalogObject e) => new CatalogDocument(e.Id.ToString(), e.NameEnglish, e.DescEnglish);
    static CatalogDocument GetFrenchDocument(CatalogObject e) => new CatalogDocument(e.Id.ToString(), e.NameFrench, e.DescFrench);
}
