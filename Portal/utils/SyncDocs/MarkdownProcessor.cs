using Datahub.Markdown;
using Datahub.Markdown.Model;
using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Datahub.Shared.Annotations;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
namespace SyncDocs;

internal class MarkdownProcessor
{
	private readonly ConfigParams _config;
	private readonly string _sourcePath;
	private readonly DocTranslationService _translationService;
    private readonly DictionaryCache _fileNameCache;
    private readonly FileMappingService _mappingService;

    public MarkdownProcessor(ConfigParams config, string sourcePath, DocTranslationService translationService, 
        DictionaryCache fileNameCache, FileMappingService mappingService)
	{
		_config = config;
        _sourcePath = sourcePath;
        _translationService = translationService;
        _fileNameCache = fileNameCache;
        _mappingService = mappingService;
    }

	public Task CreateTranslatedFolder(string relative, string path) 
	{
        var outputFolder = GetTargetPath(path);
        if (!Directory.Exists(outputFolder))
		{
            Directory.CreateDirectory(outputFolder);
        }
        return Task.CompletedTask;
    }

    static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    public IDictionary<string, string> ValidationErrors { get; set; } = new ConcurrentDictionary<string,string>();

    private static DocumentationGuideRootSection? GetSection(string relPath)
    {
        var currentSection = relPath.Split("/", StringSplitOptions.RemoveEmptyEntries)[1];
        var sections = Enum.GetValues<DocumentationGuideRootSection>().Select(v => (section:v,path: v.GetStringValue())).ToList();
        if (!sections.Any(s => s.path == currentSection))
            return null;
        return sections.First(s => s.path == currentSection).section;
    }

    public async Task ValidateFile(string relative, string path)
    {
        var sourceFileName = Path.GetFileName(path);

        var section = GetSection(relative);
        var isSidebar = Path.GetFileName(path) == "_sidebar.md";
        if (!isSidebar && section == DocumentationGuideRootSection.UserGuide)
        {
            var content = await File.ReadAllTextAsync(path);
            if (!ExternalPageMarkdown.IsExternalMarkdown(content))
            {
                var card = MarkdownTools.GetTitleAndPreview(content);
                if (card is null)
                {
                    ValidationErrors.TryAdd(Path.GetFullPath(path), $"Invalid markdown structure: first Header or first Paragraph missing for card preview");
                }
            }
        }
    }


    public async Task TranslateFile(string relative, string path)
    {
        var sourceFileName = Path.GetFileName(path);

        // remap the path
        var sourcePath = path[..^sourceFileName.Length];
        
        var outputPath = GetTargetPath(sourcePath);

        // handle file name translation
        string translatedName = await TranslateFileName(sourceFileName);

        // build output path
        //var outputFilePath = Path.Combine(outputPath, RemoveDiacritics(translatedName));
        var outputFilePath = Path.Combine(outputPath, translatedName);

        // add file mapping
        AddFileMapping(relative, path, outputFilePath);
        var isSidebar = Path.GetFileName(outputFilePath) == "_sidebar.md";
        if (!File.Exists(outputFilePath) || CheckIfDraft(outputFilePath) || isSidebar)
        {
            var outputFile = await _translationService.TranslateMarkupFile(path, outputFilePath, isSidebar);
            Console.WriteLine($"+ {(outputFile is null ? ("no client available for " + outputFilePath):Path.GetFullPath(outputFile))}");
        }
    }

    private static MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();

    internal static bool CheckIfDraft(string outputFilePath)
    {
        var content = File.ReadAllText(outputFilePath);
        var document = Markdown.Parse(content, pipeline);
        // extract the front matter from markdown document
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock is null)
            return true;
        var yaml = yamlBlock.Lines.ToString();

        // deserialize the yaml block into a custom type
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var metadata = deserializer.Deserialize<Dictionary<string, string>>(yaml);
        if (metadata.TryGetValue("draft", out var draftString))
        {
            var isDraft = Boolean.Parse(draftString);
            //Console.WriteLine($"File {outputFilePath} - Draft={isDraft}");
            return isDraft;
        }
        return false;
    }

    internal static bool CheckIfAutogenerateFrontMatter(string outputFilePath)
    {
        if (!File.Exists(outputFilePath))
            return true;
        var content = File.ReadAllText(outputFilePath);
        var document = Markdown.Parse(content, pipeline);
        // extract the front matter from markdown document
        var yamlBlock = document.Descendants<YamlFrontMatterBlock>().FirstOrDefault();
        if (yamlBlock is null)
            return true;
        var yaml = yamlBlock.Lines.ToString();

        // deserialize the yaml block into a custom type
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var metadata = deserializer.Deserialize<Dictionary<string, string>>(yaml);
        if (metadata.TryGetValue("autogenerate", out var draftString))
        {
            var isDraft = Boolean.Parse(draftString);
            //Console.WriteLine($"File {outputFilePath} - Draft={isDraft}");
            return isDraft;
        }
        return false;
    }

    internal static bool CheckIfAutogenerateYaml(string outputFilePath)
    {
        if (!File.Exists(outputFilePath))
            return true;
        var content = File.ReadAllText(outputFilePath);
        // deserialize the yaml block into a custom type
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var metadata = deserializer.Deserialize<Dictionary<string, string>>(content);
        if (metadata.TryGetValue("autogenerate", out var draftString))
        {
            var isDraft = Boolean.Parse(draftString);
            //Console.WriteLine($"File {outputFilePath} - Draft={isDraft}");
            return isDraft;
        }
        return false;
    }

    private static Guid GenerateGuid(string input)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
            return new Guid(hash);
        }
    }
    private void AddFileMapping(string relative, string engPath, string frePath)
    {
        var id = GenerateGuid(relative).ToString();
        var engUrl = ExtractUrlFromPath(engPath);
        var freUrl = ExtractUrlFromPath(frePath);
        _mappingService.AddPair(id, engUrl, freUrl);
    }

    private string ExtractUrlFromPath(string path)
    {
        var url = path[_sourcePath.Length..].Replace('\\', '/');
        return url.StartsWith('/') ? url : $"/{url}";
    }

    private async Task<string> TranslateFileName(string sourceFileName)
    {
        var translatedName = sourceFileName;
        if (!sourceFileName.StartsWith('_'))
        {
            var cachedTranslation = _fileNameCache.GetFrenchTranslation(sourceFileName);
            if (cachedTranslation is not null)
            {
                translatedName = cachedTranslation;
            }
            else
            {
                translatedName = await _translationService.TranslateFileName(sourceFileName);
                _fileNameCache.SaveFrenchTranslation(sourceFileName, translatedName);
            }
        }
        return translatedName;
    }


    private string GetTargetPath(string path)
	{
		var relativePath = path[_sourcePath.Length..];
        if (relativePath.StartsWith('/') || relativePath.StartsWith('\\'))
            relativePath = relativePath[1..];
        return Path.Combine(_sourcePath, _config.Target, relativePath);
    }
}
