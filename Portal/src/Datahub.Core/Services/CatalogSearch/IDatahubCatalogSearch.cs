using Datahub.Core.Model.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Datahub.Core.Services.CatalogSearch;

public interface IDatahubCatalogSearch
{
    Task<List<CatalogObject>> SearchCatalog(DatahubSearchRequest query);
    Task AddCatalogObject(CatalogObject value);
}