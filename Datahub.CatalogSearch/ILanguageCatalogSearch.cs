namespace Datahub.CatalogSearch;
public interface ILanguageCatalogSearch
{
    /// <summary>
    /// Adds a new document to the search catalog.
    /// </summary>
    /// <param name="id">External document id.</param>
    /// <param name="content">Document context to index.</param>
    void AddDocument(string id, string title, string content);
    /// <summary>
    /// Search documents matching text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxResults"></param>
    /// <returns></returns>
    IEnumerable<string> SearchDocuments(string text, int maxResults);
    /// <summary>
    /// Find auto-complete suggestions for a text.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="maxResults"></param>
    /// <returns></returns>
    IEnumerable<string> GetAutocompleteSuggestions(string text, int maxResults);
    /// <summary>
    /// Flush Indexes to storage
    /// </summary>
    void FlushIndexes();
}
