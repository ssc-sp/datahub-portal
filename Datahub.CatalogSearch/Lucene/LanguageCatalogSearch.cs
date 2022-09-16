using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;
using System.Reflection;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Documents;
using Lucene.Net.Search;
using Lucene.Net.QueryParsers.Classic;
using Datahub.CatalogSearch.Utils;

namespace Datahub.CatalogSearch.Lucene
{
    public class LanguageCatalogSearch : ILanguageCatalogSearch
    {
        const LuceneVersion LUCENE_VERSION = LuceneVersion.LUCENE_48;
        const string ID_FIELD = "id";
        const string CONTENT_FIELD = "content";

        private readonly IndexWriter _writer;
        private readonly QueryParser _queryParser;

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
            _queryParser = new QueryParser(LUCENE_VERSION, CONTENT_FIELD, standardAnalyzer);
        }

        public void AddDocument(string id, string content)
        {
            var doc = new Document();
            doc.Add(new StringField(ID_FIELD, id, Field.Store.YES));
            doc.Add(new TextField(CONTENT_FIELD, content, Field.Store.YES));
            _writer.AddDocument(doc);
        }

        public IEnumerable<string> SearchDocuments(string text, int maxResults)
        {
            using DirectoryReader reader = _writer.GetReader(applyAllDeletes: true);
            var searcher = new IndexSearcher(reader);

            Query query = _queryParser.Parse(text);
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

            Query query = _queryParser.Parse(text);
            TopDocs topDocs = searcher.Search(query, maxResults);

            var hits = topDocs.ScoreDocs.Select(d => searcher.Doc(d.Doc).Get(CONTENT_FIELD)).ToList();

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
}
