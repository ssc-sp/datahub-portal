﻿// Console app will replicate all documentation folder structure and try to translate the content of .md files using DeepL

// Usage:
// $ syncdocs gensidebars -P <path> -p ssc
// $ syncdocs translate -P <path> -p ssc -f

// Note: Expects an enviroment variable named "DEEPL_KEY" with the API key for DeepL

using SyncDocs;
using Microsoft.Extensions.Configuration;
using CommandLine;
using Microsoft.Extensions.Options;

const string SIDEBAR = "_sidebar.md";
const string SIDEBAR_META = "_sidebar.md.yaml";

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false)
    .AddUserSecrets<Program>();

var config = builder.Build();

// read the app config
var configParams = config.Get<ConfigParams>() ?? new();

await (await Parser.Default.ParseArguments<TranslateOptions, GensidebarOptions>(args)
.WithParsedAsync<TranslateOptions>(async options => 
{
    try
    {
        // pick the deepL key
        var deeplKey = options.DeeplKey ?? config.GetSection("DeepL")?.GetValue<string>("Key") ?? Environment.GetEnvironmentVariable("DEEPL_KEY") ?? string.Empty;

        // file name cache
        var fileNameCache = new DictionaryCache(Path.Combine(options.Path, configParams.Target, "filenamecache.json"));
        // file name cache
        var translationCache = new DictionaryCache(Path.Combine(options.Path, configParams.Target, "translationcache.json"));

        // file mapping service
        var fileMappingService = new FileMappingService(Path.Combine(options.Path, "filemappings.json"));

        // delete unnecessary files
        fileMappingService.CleanUpMappings();

        // translation service
        var translationService = new TranslationService(options.Path, deeplKey, options.UseFreeAPI, translationCache, GetGlossary(options.Path).ToList());

        // replication service
        var markdownService = new MarkdownDocumentationService(configParams, options.Path, translationService, fileNameCache, fileMappingService);

        await CleanupTargetDirectory(configParams, options.Path);
        // iterate the provided source folder
        await IteratePath(options.Path, BuildExcluder(configParams), markdownService.AddFolder, markdownService.AddFile);

        // save translation cache
        fileNameCache.SaveChanges();

        // save the file mappings
        fileMappingService.SaveMappings();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing translations: {ex.Message}");
    }
}))
.WithParsedAsync<GensidebarOptions>(async options => 
{
    try
    {
        // replication service
        var topLevelfolders = new List<string>();
        var topLevelfiles = new List<string>();
        // iterate the provided source folder
        Func<string,bool> excluder = n => Path.GetFileName(n) == SIDEBAR || BuildExcluder(configParams)(n);
        await IteratePath(options.Path, excluder, AddAsync(topLevelfolders!), AddAsync(topLevelfiles), options.TopLevelDepth);
        var gen = new SidebarGenerator();
        var topSidebar = gen.GenerateTopLevel(options.Path, topLevelfiles, topLevelfolders, options.Profile);
        Console.WriteLine($"Processing top level directory {options.Path}");
        var metadata = "autogenerate: true\n";
        var topSidebarPath = Path.Combine(options.Path, SIDEBAR);
        var topSidebarPathMeta = Path.Combine(options.Path, SIDEBAR_META);
        if (MarkdownDocumentationService.CheckIfAutogenerateYaml(topSidebarPathMeta))
        {
            await File.WriteAllTextAsync(topSidebarPath, topSidebar);
            await File.WriteAllTextAsync(topSidebarPathMeta, metadata);
            Console.WriteLine($"Generated top level sidebar");
        }
        var topFolders1 = topLevelfolders.Where(s => FolderDepth(Path.GetRelativePath(options.Path, s)) < options.TopLevelDepth).ToList();

        foreach (var folder in topFolders1)
        {
            Console.WriteLine($"Processing {folder}");
            var folders = new List<string>();
            var files = new List<string>();
            await IteratePath(folder, excluder, AddAsync(folders), AddAsync(files));
            Console.WriteLine($"Found {files.Count} files for sidebar");
            var sidebar = gen.GenerateSidebar(new DirectoryInfo(folder).Name, folder, files, folders, options.Profile);
            var sidebarPath = Path.Combine(folder, SIDEBAR);
            var sidebarPathMeta = Path.Combine(folder, SIDEBAR_META);
            if (MarkdownDocumentationService.CheckIfAutogenerateYaml(sidebarPathMeta))
            {
                await File.WriteAllTextAsync(sidebarPath, sidebar);
                await File.WriteAllTextAsync(sidebarPathMeta, metadata);
                Console.WriteLine($"Generated sidebar {sidebarPath}");
            }
        }
    } 
    catch (Exception ex)
    {
        Console.WriteLine($"Error processing sidebar: {ex.Message}");
    }
});

#region util functions

static async Task IteratePath(string path, Func<string, bool> excludeFunc, Func<string,Task> addFolder, Func<string,Task> addFile, int maxRecursion = int.MaxValue, int recursionLevel = 0)
{
    foreach (var dir in Directory.GetDirectories(path))
    {
        var name = Path.GetFileName(dir);

        // ignore all .* folders
        // check for excluded
        if (!excludeFunc.Invoke(name) && !name.StartsWith('.'))
        {

            await addFolder(dir);

            if (recursionLevel < maxRecursion)
                // check for exclusions
                await IteratePath(dir, n => false, addFolder, addFile, maxRecursion, recursionLevel + 1);
        }
    }

    foreach (var file in Directory.GetFiles(path, "*.md"))
    {
        if (!excludeFunc.Invoke(file))
            await addFile(file);
    }
}

static async Task CleanupTargetDirectory(ConfigParams configParams, string path)
{
    var folders = new List<string>();
    var files = new List<string>();
    //cleanup French folder
    await IteratePath(Path.Combine(path, configParams.Target), s => false, f => { folders.Add(f); return Task.CompletedTask; }, f => { files.Add(f); return Task.CompletedTask; });
    foreach (var item in files.Where(f => Path.GetExtension(f) == ".md"))
    {   
        if (MarkdownDocumentationService.CheckIfDraft(item))
        {
            Console.WriteLine($"Deleting draft file {item}");
            File.Delete(item);
        }
    }
    foreach (var folder in folders.OrderByDescending(f => f.Split(Path.DirectorySeparatorChar).Count()))
    {
        if (Directory.EnumerateFiles(folder).Count() == 0)
        {
            Console.WriteLine($"Deleting empty folder {folder}");
            Directory.Delete(folder);               
        }
    }
}

static Func<string, bool> BuildExcluder(ConfigParams config)
{
    var excludeSet = config.GetExcludedFolders().Select(s => s.ToLower()).ToHashSet();
    return n => excludeSet.Contains(n.ToLower());
}

static int FolderDepth(string path)
{
    var depth = 0;
    foreach (var c in path)
    {
        if (c == '\\' || c == '/')
            depth++;
    }
    return depth;
}

static Func<string, Task> AddAsync(List<string> list)
{
    return s =>
    {
        list.Add(s);
        return Task.CompletedTask;
    };
}

static IEnumerable<GlossaryTerm> GetGlossary(string basePath)
{
    var glossaryPath = Path.Combine(basePath, "glossary.csv");
    
    if (!File.Exists(glossaryPath))
        yield break;

    foreach (var line in File.ReadLines(glossaryPath))
    {
        var terms = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
        if (terms.Length == 2)
            yield return new(terms[0], terms[1]);
    }
}

#endregion

[Verb("translate", HelpText = "Translate the documentation")]
public class TranslateOptions
{
    [Option('P', "path", Required = false)]
    public string Path { get; set; } = ".";

    [Option("deepl", Required = false)]
    public string? DeeplKey { get; set; } = null;

    [Option('f',"FreeAPI", Required = false)]
    public bool UseFreeAPI { get; set; } = false;

    [Option('p', "profile", Required = true)]
    public string Profile { get; set; } = "ssc";
}

[Verb("gensidebars", HelpText = "Generate sidebar files")]
public class GensidebarOptions
{
    [Option('P', "path", Required = false)]
    public string Path { get; set; } = ".";

    [Option('t', "topLevel", Required = false)]
    public int TopLevelDepth { get; set; } = 1;
    
    [Option('p', "profile", Required = true)]
    public string Profile { get; set; } = "ssc";
}
