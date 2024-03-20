using DeepL;
using Markdig.Extensions.Yaml;
using Markdig.Renderers.Roundtrip;
using Markdig;
using Markdig.Syntax;
using System.Text.RegularExpressions;

namespace SyncDocs;

internal class DocTranslationService
{
    private readonly Translator? _client;
    private readonly string _sourcePath;
    private readonly DictionaryCache _translationCache;
    private readonly List<GlossaryTerm> _glossaryTerms;

    public DocTranslationService(string sourcePath, string deeplKey, bool useFreeAPI, 
        DictionaryCache translationCache, List<GlossaryTerm> glossaryTerms)
    {
        _sourcePath = sourcePath;
        _translationCache = translationCache;
        _client = !string.IsNullOrEmpty(deeplKey) ? new Translator(deeplKey, new TranslatorOptions() { ServerUrl = useFreeAPI ? "https://api-free.deepl.com" : null }) : null;
        _glossaryTerms = glossaryTerms;
    }

    public async Task<string> TranslateFileName(string fileName)
    {
        if (_client is null)
            return fileName;

        var name = Path.GetFileNameWithoutExtension(fileName);
        var ext = Path.GetExtension(fileName);
        var tranlatedName = await Translate(CleanName(name));

        return $"{AssembleName(tranlatedName, '-')}{ext}";
    }

    private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
        .UseYamlFrontMatter().EnableTrackTrivia().Build();

    public static string RemoveFrontMatter(string input)
    {
        var document = Markdown.Parse(input, pipeline);
        var sw = new StringWriter();
        var renderer = new RoundtripRenderer(sw);
        // extract the front matter from markdown document
        var yamlBlocks = document.Descendants<YamlFrontMatterBlock>();

        foreach (var block in yamlBlocks)
        {
            block.Lines = new Markdig.Helpers.StringLineGroup();
        }
        renderer.Write(document);
        return sw.ToString();
    }

    public async Task<string?> TranslateMarkupFile(string sourcePath, string outputPath, bool isSidebar)
    {
        if (_client is null)
            return null;

        var sourceFileUrl = GetSourceFilePathUrl(sourcePath);
        var metadata = $"remarks: Automatically translated with DeepL\nsource: {sourceFileUrl}\ndraft: true\n";
        //var remarks = $"[_metadata_: remarks]:- \"Automatically translated with DeepL. From: {sourceFileUrl}\"";
        //var note = $"[_(draft documentation, please review)_]({sourceFileUrl})";
        //var note = $"_(draft documentation, please review)_";

        using var writer = new StreamWriter(outputPath);
        if (!isSidebar)
        {
            writer.WriteLine($"---\n{metadata}---");
            writer.WriteLine();
        }
        else
        {
            File.WriteAllText($"{outputPath}.yaml", metadata);
        }

        //writer.WriteLine(note);
        //writer.WriteLine();
        var srcContent = RemoveFrontMatter(File.ReadAllText(sourcePath));
        using StringReader sr = new StringReader(srcContent);
        string? line;
        while ((line = sr.ReadLine()) != null)
        {
            // do something
            if (string.IsNullOrWhiteSpace(line))
            {
                writer.WriteLine();
                continue;
            }
            var translated = isSidebar ? await TranslateSidebar(line, "/fr") : await Translate(line);
            writer.WriteLine(translated);
        }
        return outputPath;
    }

    private async Task<string> TranslateSidebar(string text, string relPath)
    {
        var link = TryParseLink(text);
        if (link == null)
        {
            var translation = await Translate(text);
            return (translation ?? "").Replace("] (", "](");
        }
        else
        {
            var heading = await Translate(link.Heading);
            var path = await TranslateFilePath(link.Path);
            return string.Format(link.Template, heading, $"{relPath}{(path.StartsWith("/") ? "" : "/")}{path}");
        }
    }

    private async Task<string> TranslateFilePath(string filePath)
    {
        var fileName = Path.GetFileName(filePath);
        var pathOnly = filePath[..(filePath.Length - fileName.Length)];
        var fileNameTranslated = await TranslateFileName(fileName);
        return $"{pathOnly}{fileNameTranslated}";
    }

    private string GetSourceFilePathUrl(string filePath)
    {
        var relPath = filePath.Substring(_sourcePath.Length).Replace('\\', '/');
        return !relPath.StartsWith('/') ? $"/{relPath}" : relPath;
    }


    private async Task<string> Translate(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        string replaced = TranslateURLs(text);

        var output = _translationCache.GetFrenchTranslation(replaced);
        if (output != null)
            return ApplyGlossary(output);

        var translateResult = await _client!.TranslateTextAsync(replaced, LanguageCode.English, LanguageCode.French);
        var result = ApplyGlossary(translateResult?.Text ?? replaced);

        _translationCache.SaveFrenchTranslation(replaced, result);
        _translationCache.SaveChanges();

        return result;
    }


    private static readonly Regex MS_LEARN_URL = new Regex(@"https://learn\.microsoft\.com/(\w\w)-(\w\w)(/[^/]+)(.*)", RegexOptions.Compiled);
    private static readonly Regex STATSCAN_LECTURES = new Regex(@"https://www\.statcan\.gc\.ca/en/wtc/online-lectures/(.*)", RegexOptions.Compiled);
    //https://www.statcan.gc.ca/en/wtc/online-lectures/qgis/2020020
    //https://www.statcan.gc.ca/fr/afc/cours-en-ligne/qgis/2020020
    internal static string TranslateURLs(string text)
    {
        var statsCan = STATSCAN_LECTURES.Replace(text, $"https://www.statcan.gc.ca/fr/afc/cours-en-ligne/$1");
        return MS_LEARN_URL.Replace(statsCan, $"https://learn.microsoft.com/fr-ca$3$4");
    }

    private string ApplyGlossary(string text)
    {
        var result = text ?? string.Empty;
        foreach (var term in _glossaryTerms)
        {
            result = result.Replace(term.English, term.French);
        }
        return result;
    }

    private string AssembleName(string name, char sep)
    {
        var words = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(sep, words);
    }

    private string CleanName(string name)
    {
        return string.Join("", EnumerateValidChars(name));
    }

    private IEnumerable<char> EnumerateValidChars(string name)
    {
        foreach (var c in name)
        {
            if (char.IsLetterOrDigit(c))
                yield return c;
            else
                yield return ' ';
        }
    }

    static LinkData? TryParseLink(string text)
    {
        // match [head]
        var (index1, index2) = GetDelimitedRange(text, '[', ']', 0);
        if (index1 < 0)
            return null;

        // match (path)
        var (index3, index4) = GetDelimitedRange(text, '(', ')', index2);
        if (index3 < 0)
            return null;

        var template = string.Concat(text[..(index1 + 1)], "{0}", text[index2..(index3 + 1)], "{1}", text[index4..]);
        var heading = text[(index1 + 1)..index2];
        var path = text[(index3 + 1)..index4];

        return new LinkData(template, heading, path);
    }

    static (int start, int end) GetDelimitedRange(string text, char startChar, char endChar, int startIndex)
    {
        var start = text.IndexOf(startChar, startIndex);
        if (start >= 0)
        {
            var open = 1;
            for (var i = start + 1; i < text.Length; i++)
            {
                if (text[i] == startChar)
                {
                    open++;
                }
                else if (text[i] == endChar)
                {
                    if (--open == 0)
                        return (start, i);
                }
            }
        }
        return (-1, -1);
    }
}

record LinkData(string Template, string Heading, string Path);
