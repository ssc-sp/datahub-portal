namespace SyncDocs;

internal class MarkdownDocumentationService
{
	private readonly ConfigParams _config;
	private readonly string _sourcePath;
	private readonly TranslationService _translationService;
    private readonly FileNameCache _fileNameCache;
    private readonly FileMappingService _mappingService;

    public MarkdownDocumentationService(ConfigParams config, string sourcePath, TranslationService translationService, 
        FileNameCache fileNameCache, FileMappingService mappingService)
	{
		_config = config;
        _sourcePath = sourcePath;
        _translationService = translationService;
        _fileNameCache = fileNameCache;
        _mappingService = mappingService;
    }

	public Task AddFolder(string path) 
	{
        var outputFolder = GetTargetPath(path);
        if (!Directory.Exists(outputFolder))
		{
            Directory.CreateDirectory(outputFolder);
        }
        return Task.CompletedTask;
    }

    public async Task AddFile(string path)
    {
        var sourceFileName = Path.GetFileName(path);

        // remap the path
        var sourcePath = path[..^sourceFileName.Length];
        
        var outputPath = GetTargetPath(sourcePath);

        // handle file name translation
        string translatedName = await TranslateFileName(sourceFileName);

        // build output path
        var outputFilePath = Path.Combine(outputPath, translatedName);

        // add file mapping
        AddFileMapping(path, outputFilePath);

        if (!File.Exists(outputFilePath))
        {
            Console.WriteLine($"+ {outputFilePath}");
            await _translationService.TranslateMarkupFile(path, outputFilePath, "_sidebar.md".Equals(translatedName));
        }
    }

    private void AddFileMapping(string engPath, string frePath)
    {
        var id = Guid.NewGuid().ToString();
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
