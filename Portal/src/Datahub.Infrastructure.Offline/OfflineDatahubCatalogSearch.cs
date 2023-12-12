using Datahub.Core.Model.Catalog;
using Datahub.Core.Services.CatalogSearch;

namespace Datahub.Infrastructure.Offline;

public class OfflineDatahubCatalogSearch : IDatahubCatalogSearch
{
    public Task<List<CatalogObject>> SearchCatalog(DatahubSearchRequest query)
    {
        return Task.FromResult(new List<CatalogObject>());
    }

    public Task AddCatalogObject(CatalogObject value)
    {
        return Task.CompletedTask;
    }
}