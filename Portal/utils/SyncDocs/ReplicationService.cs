namespace SyncDocs;

internal class ReplicationService
{
	private readonly ConfigParams _config;
	private readonly string _sourcePath;
	private readonly TranslationService _translationService;
    private readonly FileNameCache _fileNameCache;

    public ReplicationService(ConfigParams config, string sourcePath, TranslationService translationService, FileNameCache fileNameCache)
	{
		_config = config;
        _sourcePath = sourcePath;
        _translationService = translationService;
        _fileNameCache = fileNameCache;
    }

	public void AddFolder(string path) 
	{
        var outputFolder = GetTargetPath(path);
        if (!Directory.Exists(outputFolder))
		{
            Directory.CreateDirectory(outputFolder);
        }
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
        if (!File.Exists(outputFilePath))
        {
            Console.WriteLine($"+ {outputFilePath}");
            await _translationService.TranslateMarkupFile(path, outputFilePath, "_sidebar.md".Equals(translatedName));
        }
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
