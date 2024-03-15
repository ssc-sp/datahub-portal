using Markdig;
using Markdig.Extensions.Yaml;
using Markdig.Syntax;
using System.Text;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace SyncDocs
{
    internal record DocEntry
    {
        public string? FileName { get; set; }
        public string? FullPath { get; set; }
        public string? RelativePath { get; set; }
        public DocEntry? Parent { get; set; }
        public string? Url { get; internal set; }
    }

    internal class SidebarGenerator
    {
        private const string DEFAULT_FILE = "README.md";
        
        private static readonly List<(string, string)> CasingExceptions = new List<(string, string)> { ("Power bI","PowerBI"), ("Az copy","AzCopy"), ("Po c", "PoC") };

        private static readonly MarkdownPipeline pipeline = new MarkdownPipelineBuilder().UseYamlFrontMatter().Build();
        
        internal string GenerateSidebar(string relativePath, string path, List<string> files, List<string> folders, string activeProfile)
        {
            // first sort the files
            var sorted = files.OrderBy(f => f);
            var sb = new StringBuilder();
            var entries = new Dictionary<string, DocEntry>();
            var topEntry = new DocEntry() {  FullPath = path, Url = "/" + relativePath };
            entries.Add("", topEntry);
            foreach (var file in sorted)
            {                
                var diff = Path.GetRelativePath(path, file);
                var currentPath = diff.Replace('\\', '/');
                var url = currentPath;
                //strip README.md
                if (currentPath.EndsWith(DEFAULT_FILE))
                {
                    url = currentPath.Substring(0, currentPath.Length - DEFAULT_FILE.Length);
                }
                BuildHierarchy(path, entries, currentPath, file, url,topEntry);

                //var depth = GetPathDepth(currentPath);
                //if (!string.IsNullOrWhiteSpace(currentPath))
                //    sb.AppendLine($"- [{GetTitle(file)}](/{currentPath})");
            }
            // process top level
            WriteEntries(entries,topEntry, 0, sb, activeProfile);                            
            return sb.ToString();
        }

        private bool IsIncludeOnProfile(DocEntry entry, string activeProfile)
        {
            if (entry.FullPath is null)
                return true;
            if (Directory.Exists(entry.FullPath))
                return true;
            if (!File.Exists(entry.FullPath))
                return false;
            var content = File.ReadAllText(entry.FullPath);
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
            if (metadata.TryGetValue("onProfileExclude", out var excludedValue))
            {
                var excluded = excludedValue.Split(",").Select(e => e.Trim()).ToList();
                if (excluded.Contains(activeProfile))
                    return false;
            }
            if (metadata.TryGetValue("onProfileInclude", out var includedValue))
            {
                var included = includedValue.Split(",").Select(e => e.Trim()).ToList();
                if (included.Contains(activeProfile))
                    return true;
                return false;
            }
            return true;
        }

        private bool WriteEntries(Dictionary<string, DocEntry> entries, DocEntry parent, int level, StringBuilder sb, string activeProfile)
        {
            var currentEntries = entries.Values.Where(e => e.Parent == parent && IsIncludeOnProfile(e,activeProfile)).OrderBy(e => e.RelativePath).ToList();
            foreach (var entry in currentEntries)
            {
                for (int i = 0; i < level; i++)
                {
                    sb.Append("  ");
                }
                
                if (entry.Url != null)
                {
                    sb.AppendLine($"- [{GetTitle(entry.Url ?? entry.RelativePath ?? "")}]({entry.Url})");
                }
                else
                {
                    sb.AppendLine($"- {GetTitle(entry.Url ?? entry.RelativePath ?? "")}");
                }
                var sb2 = new StringBuilder();
                if (WriteEntries(entries, entry, level + 1, sb2, activeProfile))
                {
                    sb.AppendLine();
                    sb.Append(sb2);
                }
            }
            if (currentEntries.Count > 0)
                sb.AppendLine();
            return currentEntries.Count > 0;
        }

        private void BuildHierarchy(string path, Dictionary<string, DocEntry> entries, string relativePath, string file, string url, DocEntry topEntry)
        {
            var components = relativePath.Split('/');
            for (int il =0; il < components.Length;il++)
            {
                var currentPath = components[0..(il+1)].Aggregate((a, b) => a + "/" + b);
                var parentPath = il > 0?components[0..il].Aggregate((a, b) => a + "/" + b):string.Empty;
                var filename = il == components.Length - 1 ? components[il] : null;
                var isFinal = il == components.Length - 1;
                if (isFinal && filename == DEFAULT_FILE)
                {
                    
                    var parent = entries[parentPath];
                    parent.Url = url;
                    parent.FileName = filename;
                    parent.FullPath = Path.Combine(path, currentPath);
                } else if (!entries.ContainsKey(currentPath))
                {
                    var entry = new DocEntry
                    {
                        FileName = isFinal?filename:null,
                        Url = isFinal ? $"{topEntry.Url}/{url}" : null,
                        FullPath = Path.Combine(path,currentPath),
                        RelativePath = currentPath
                        
                    };
                    if (il >= 1)
                    {
                        entry.Parent = entries[parentPath];
                    } else
                    {
                        entry.Parent = topEntry;
                    }
                    entries.Add(currentPath, entry);
                }
            }

        }

        public static string ToSentenceCase(string str)
        {
            var s = Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}").Replace("-"," ").Replace("_"," ").Trim();
            s = Regex.Replace(s, " {2,}", " ");
            foreach (var item in CasingExceptions)
            {
                s = s.Replace(item.Item1, item.Item2);
            }
            return s;
        }

        public int GetPathDepth(string path)
        {
            return path.Split('/').Length -1;
        }

        private string GetTitle(string file)
        {
            var current = Path.GetFileNameWithoutExtension(file);
            if (string.IsNullOrEmpty(current))
                current = Path.GetDirectoryName(file);
            if (Path.GetFileName(file) == DEFAULT_FILE)
                current = new DirectoryInfo(Path.Combine(file,"..")).Name; 
            return ToSentenceCase(current!);
        }

        internal string GenerateTopLevel(string path, List<string> topLevelfiles, List<string> topLevelfolders, string activeProfile)
        {
            return GenerateSidebar("",path, topLevelfiles.Where(f => Path.GetFileName(f) == DEFAULT_FILE).ToList(), topLevelfolders, activeProfile);
        }
    }
}
