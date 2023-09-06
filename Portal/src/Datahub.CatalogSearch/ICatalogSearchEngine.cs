namespace Datahub.CatalogSearch;

public interface ICatalogSearchEngine
{
    ILanguageCatalogSearch GetMetadataEnglishSearchEngine();
    ILanguageCatalogSearch GetMetadataFrenchSearchEngine();
    ILanguageCatalogSearch GetDatahubEnglishSearchEngine(Func<IEnumerable<CatalogDocument>> englishDataReader);
    ILanguageCatalogSearch GetDatahubFrenchSearchEngine(Func<IEnumerable<CatalogDocument>> frenchDataReader);
}