namespace Datahub.CatalogSearch;

public interface ICatalogSearchEngine
{
    ILanguageCatalogSearch GetEnglishSearchEngine();
    ILanguageCatalogSearch GetFrenchSearchEngine();
}