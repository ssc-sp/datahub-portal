// Console app will replicate all documentation folder structure and try to translate the content of .md files using DeepL

// Usage:
// $ syncdocs gensidebars -P <path> -p ssc
// $ syncdocs translate -P <path> -p ssc -f

// Note: Expects an enviroment variable named "DEEPL_KEY" with the API key for DeepL

using SyncDocs;
using Microsoft.Extensions.Configuration;
using CommandLine;

const string SIDEBAR = "_sidebar.md";
const string SIDEBAR_META = "_sidebar.md.yaml";
const string NAVBAR = "_navbar.md";

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("config.json", optional: false)
    .AddJsonFile("config.deepl.json", optional: false)
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
        var freeAPI = config.GetSection("DeepL")?.GetValue<bool>("UseFreeAPI") ?? options.UseFreeAPI;
        // file name cache
        var fileNameCache = new DictionaryCache(Path.Combine(options.Path, configParams.Target, "filenamecache.json"));
        // file name cache
        var translationCache = new DictionaryCache(Path.Combine(options.Path, configParams.Target, "translationcache.json"));

        // file mapping service
        var fileMappingService = new FileMappingService(Path.Combine(options.Path, "filemappings.json"));

        // delete unnecessary files
        fileMappingService.CleanUpMappings();
        if (string.IsNullOrWhiteSpace(deeplKey)) throw new InvalidOperationException($"Deepl Key is missing");
        // translation service
        var translationService = new DocTranslationService(options.Path, deeplKey, freeAPI, translationCache, GetGlossary(options.Path).ToList());
        // replication service
        var markdownProcessor = new MarkdownProcessor(configParams, options.Path, translationService, fileNameCache, fileMappingService);

        if (options.Validate)
        {
            await IteratePath(options.Path, BuildExcluder(configParams), (_,_) => Task.CompletedTask, markdownProcessor.ValidateFile);
        }

        if (options.Validate && markdownProcessor.ValidationErrors.Count > 0)
        {
            Console.Error.WriteLine("Please correct the following markdown documents:");
            foreach (var item in markdownProcessor.ValidationErrors.OrderBy(tp => tp.Key))
            {
                Console.Error.WriteLine($"{item.Key}: {item.Value}");
            }
        }
        else
        {

            await CleanupTargetDirectory(configParams, options.Path);
            // iterate the provided source folder
            await IteratePath(options.Path, BuildExcluder(configParams), markdownProcessor.CreateTranslatedFolder, markdownProcessor.TranslateFile);

            // save translation cache
            fileNameCache.SaveChanges();

            // save the file mappings
            fileMappingService.SaveMappings();
        }
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
        if (MarkdownProcessor.CheckIfAutogenerateYaml(topSidebarPathMeta))
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
            if (MarkdownProcessor.CheckIfAutogenerateYaml(sidebarPathMeta))
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
static Task IteratePath(string path, Func<string, bool> excludeFunc, Func<string,string, Task> addFolder, Func<string,string, Task> addFile, int maxRecursion = int.MaxValue, int recursionLevel = 0)
    => IteratePath2(path, path, excludeFunc, addFolder, addFile, maxRecursion, recursionLevel);

static string GetRelativePath(string current, string root)
{
    Uri path1 = new Uri(Path.GetFullPath(root));
    Uri path2 = new Uri(Path.GetFullPath(current));
    return path1.MakeRelativeUri(path2).OriginalString;
}

static async Task IteratePath2(string root, string path, Func<string, bool> excludeFunc, Func<string,string,Task> addFolder, Func<string,string,Task> addFile, int maxRecursion = int.MaxValue, int recursionLevel = 0)
{
    foreach (var dir in Directory.GetDirectories(path))
    {
        var name = Path.GetFileName(dir);

        // ignore all .* folders
        // check for excluded
        if (!excludeFunc.Invoke(name) && !name.StartsWith('.'))
        {
            var relativeFolder = GetRelativePath(dir, root);
            await addFolder(relativeFolder, dir);

            if (recursionLevel < maxRecursion)
                // check for exclusions
                await IteratePath2(root, dir, n => false, addFolder, addFile, maxRecursion, recursionLevel + 1);
        }
    }

    foreach (var file in Directory.GetFiles(path, "*.md"))
    {
        if (!excludeFunc.Invoke(file))
        {
            var relativePath = GetRelativePath(file, root);
            await addFile(relativePath, file);
        }
    }
}

static async Task CleanupTargetDirectory(ConfigParams configParams, string path)
{
    var folders = new List<string>();
    var files = new List<string>();
    //cleanup French folder
    await IteratePath(Path.Combine(path, configParams.Target), s => false, (_,f) => { folders.Add(f); return Task.CompletedTask; }, (_,f) => { files.Add(f); return Task.CompletedTask; });
    foreach (var item in files.Where(f => Path.GetExtension(f) == ".md"))
    {   
        if (MarkdownProcessor.CheckIfDraft(item) && !string.Equals(Path.GetFileName(item), NAVBAR, StringComparison.InvariantCultureIgnoreCase))
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
    return n => excludeSet.Contains(n.ToLower()) || string.Equals(Path.GetFileName(n),NAVBAR,StringComparison.InvariantCultureIgnoreCase);
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

static Func<string, string, Task> AddAsync(List<string> list)
{
    return (relative,s) =>
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

    [Option('v', "validate", Required = false)]
    public bool Validate { get; set; } = true;

    [Option("deepl", Required = false)]
    public string? DeeplKey { get; set; } = null;

    [Option('f',"FreeAPI", Required = false)]
    public bool UseFreeAPI { get; set; } = false;

    [Option('p', "profile", Required = true)]
    public string Profile { get; set; } = "ssc";
}

[Verb("validate", HelpText = "Validate the documentation")]
public class ValidateOptions
{
    [Option('P', "path", Required = false)]
    public string Path { get; set; } = ".";

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
