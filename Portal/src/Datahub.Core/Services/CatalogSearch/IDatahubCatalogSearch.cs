using Datahub.Core.Model.Catalog;

namespace Datahub.Core.Services.CatalogSearch;

public interface IDatahubCatalogSearch
{
    Task<List<CatalogObject>> SearchCatalog(DatahubSearchRequest query);
    Task AddCatalogObject(CatalogObject value);
}