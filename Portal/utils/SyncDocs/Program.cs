// Console app will replicate all documentation folder structure and try to translate the content of .md files using DeepL

// Usage > syncdocs <root-path>
// or    > dotnet run <root-path>

// Note: Expects an enviroment variable named "DEEPL_KEY" with the API key for DeepL

using SyncDocs;
using Microsoft.Extensions.Configuration;
using CommandLine.Text;
using CommandLine;
using System.Runtime.InteropServices;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false)
    .AddUserSecrets<Program>();

var config = builder.Build();

// read the app config
var configParams = config.Get<ConfigParams>() ?? new();

Parser.Default.ParseArguments<TranslateOptions, GensidebarOptions>(args)
.WithParsed<TranslateOptions>(async options => {
    var deeplKey = options.DeeplKey ?? config.GetSection("DeepL")?.GetValue<string>("Key") ?? Environment.GetEnvironmentVariable("DEEPL_KEY") ?? string.Empty;
    // file name cache
    var fileNameCache = new FileNameCache(Path.Combine(options.Path, configParams.Target, "filenamecache.json"));

    // file mapping service
    var fileMappingService = new FileMappingService();

    // translation service
    var translationService = new TranslationService(options.Path, deeplKey);

    // replication service
    var markdownService = new MarkdownDocumentationService(configParams, options.Path, translationService, fileNameCache, fileMappingService);

    // iterate the provided source folder
    await IteratePath(options.Path, BuildExcluder(configParams), markdownService.AddFolder, markdownService.AddFile);

    // save translation cache
    fileNameCache.SaveChanges();

    // save the file mappings
    fileMappingService.SaveTo(Path.Combine(options.Path, "filemappings.json"));

    // END
})
.WithParsed<GensidebarOptions>(async options => {
    // replication service
    var topLevelfolders = new List<string>();
    var topLevelfiles = new List<string>();
    // iterate the provided source folder
    await IteratePath(options.Path, BuildExcluder(configParams), async f => topLevelfolders.Add(f), async f => topLevelfiles.Add(f), options.TopLevelDepth);
    var gen = new SidebarGenerator();
    var topSidebar = gen.GenerateTopLevel(options.Path, topLevelfiles, topLevelfolders);
    await File.WriteAllTextAsync(Path.Combine(options.Path, "_sidebar.md"),topSidebar);
    var topFolders1 = topLevelfolders.Select(s => SidebarGenerator.FolderDepth(Path.GetRelativePath(options.Path,s))).ToList();
    
    foreach (var folder in topFolders1)
    {

    }
});




#region util functions

static async Task IteratePath(string path, Func<string, bool> excludeFunc, Func<string,Task> addFolder, Func<string,Task> addFile, int maxRecursion = int.MaxValue, int recursionLevel = 0)
{
    foreach (var dir in Directory.GetDirectories(path))
    {
        var name = Path.GetFileName(dir);

        // ignore all .* folders
        if (name.StartsWith('.'))
            continue;

        // check for excluded
        if (excludeFunc.Invoke(name))
            continue;

        await addFolder(dir);

        if (recursionLevel < maxRecursion)
        // check for exclusions
            await IteratePath(dir, n => false, addFolder, addFile, maxRecursion, recursionLevel+1);
    }

    foreach (var file in Directory.GetFiles(path, "*.md"))
    {
        await addFile(file);
    }
}

static Func<string, bool> BuildExcluder(ConfigParams config)
{
    var excludeSet = config.GetExcludedFolders().Select(s => s.ToLower()).ToHashSet();
    return n => excludeSet.Contains(n.ToLower());
}

#endregion

[Verb("translate", HelpText = "Translate the documentation")]
public class TranslateOptions
{
    [Option('p', "path", Required = false)]
    public string Path { get; set; } = ".";

    [Option("deepl", Required = false)]
    public string? DeeplKey { get; set; } = null;

}

[Verb("gensidebars", HelpText = "Generate sidebar files")]
public class GensidebarOptions
{
    [Option('p', "path", Required = false)]
    public string Path { get; set; } = ".";

    [Option('t', "topLevel", Required = false)]
    public int TopLevelDepth { get; set; } = 1;

    [Option('t', "topLevel", Required = false)]
    public string Profile { get; set; } = "ssc";
    
}