using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Reflection;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Datahub.CatalogSearch.Utils;
namespace Datahub.CatalogSearch.Lucene;

public class LanguageCatalogSearch : ILanguageCatalogSearch
{
    const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;
    const string ID_FIELD = "id";
    const string TITLE_FIELD = "title";
    const string CONTENT_FIELD = "content";

    private readonly IndexWriter _writer;
    private readonly QueryParser _titleParser;
    private readonly QueryParser _contentParser;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="language">Only supports: English (en) or French (fr).</param>
    public LanguageCatalogSearch(string language)
    {
        // read stopwords
        var stopWordReader = LoadStopWords(language);

        // create the anylyzer
        var standardAnalyzer = new StandardAnalyzer(LUCENE_VERSION, stopWordReader);

        // create an index writer config
        IndexWriterConfig indexConfig = new(LUCENE_VERSION, standardAnalyzer)
        {
            OpenMode = OpenMode.CREATE
        };

        // init index writer (in-memory)
        _writer = new IndexWriter(new RAMDirectory(), indexConfig);

        // init query parser
        _titleParser = new QueryParser(LUCENE_VERSION, TITLE_FIELD, standardAnalyzer);
        _contentParser = new QueryParser(LUCENE_VERSION, CONTENT_FIELD, standardAnalyzer);
    }

    public void AddDocument(string id, string title, string content)
    {
        var doc = new Document();
        doc.Add(new StringField(ID_FIELD, id, Field.Store.YES));

        if (!string.IsNullOrEmpty(title))
        {
            var titleField = new TextField(TITLE_FIELD, title, Field.Store.YES) { Boost = 2 };
            doc.Add(titleField);
        }

        if (!string.IsNullOrEmpty(content))
        {
            var contentField = new TextField(CONTENT_FIELD, content, Field.Store.YES);
            doc.Add(contentField);
        }

        _writer.AddDocument(doc);
    }

    public IEnumerable<string> SearchDocuments(string text, int maxResults)
    {
        using DirectoryReader reader = _writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        var query = new BooleanQuery();
        var titleQuery = _titleParser.Parse(text + "*");
        var contentQuery = _contentParser.Parse(text + "*");
        query.Add(titleQuery, Occur.SHOULD);
        query.Add(contentQuery, Occur.SHOULD);

        TopDocs topDocs = searcher.Search(query, maxResults);
        foreach (var d in topDocs.ScoreDocs)
        {
            Document resultDoc = searcher.Doc(d.Doc);
            yield return resultDoc.Get(ID_FIELD);
        }
    }

    public IEnumerable<string> GetAutocompleteSuggestions(string text, int maxResults)
    {
        using DirectoryReader reader = _writer.GetReader(applyAllDeletes: true);
        var searcher = new IndexSearcher(reader);

        Query query = _titleParser.Parse(text);
        TopDocs topDocs = searcher.Search(query, maxResults);

        var hits = topDocs.ScoreDocs.Select(d => searcher.Doc(d.Doc).Get(TITLE_FIELD)).ToList();

        return AutocompleteUtils.GetSuggestions(hits, text).ToList();
    }

    public void FlushIndexes()
    {
        _writer.Commit();
    }

    private TextReader LoadStopWords(string language)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Datahub.CatalogSearch.Lucene.stopwords_{language.txt}";
        var stream = assembly?.GetManifestResourceStream(resourceName);
        return new StreamReader(stream ?? new MemoryStream());
    }
}

