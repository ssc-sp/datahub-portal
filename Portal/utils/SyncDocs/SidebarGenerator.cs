using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SyncDocs
{
    internal class SidebarGenerator
    {
        private const string DEFAULT_FILE = "README.md";

        internal string GenerateSidebar(string path, List<string> topLevelfiles, List<string> topLevelfolders)
        {
            var sb = new StringBuilder();
            foreach (var file in topLevelfiles)
            {                
                var diff = Path.GetRelativePath(path, file);
                var url = diff.Replace('\\', '/');
                if (url.EndsWith(DEFAULT_FILE))
                {
                    url = url.Substring(0, url.Length - DEFAULT_FILE.Length);
                }
                var depth = GetPathDepth(url);
                if (!string.IsNullOrWhiteSpace(url))
                    sb.AppendLine($"- [{GetTitle(file)}](/{url})");
            }
            return sb.ToString();
        }

        public static int FolderDepth(string path)
        {
            if (string.IsNullOrEmpty(path))
                return 0;
            return FolderDepth(new DirectoryInfo(path));
        }

        public static int FolderDepth(DirectoryInfo directory)
        {
            if (directory == null)
                return 0;
            return FolderDepth(directory.Parent) + 1;
        }

        public static string ToSentenceCase(string str)
        {
            return Regex.Replace(str, "[a-z][A-Z]", m => $"{m.Value[0]} {char.ToLower(m.Value[1])}");
        }

        public int GetPathDepth(string path)
        {
            return path.Split('/').Length -1;
        }

        private string GetTitle(string file)
        {
            var current = Path.GetFileNameWithoutExtension(file);
            if (Path.GetFileName(file) == DEFAULT_FILE)
                current = new DirectoryInfo(Path.Combine(file,"..")).Name; 
            return ToSentenceCase(current!);
        }

        internal string GenerateTopLevel(string path, List<string> topLevelfiles, List<string> topLevelfolders)
        {
            return GenerateSidebar(path, topLevelfiles.Where(f => Path.GetFileName(f) == DEFAULT_FILE).ToList(), topLevelfolders);
        }
    }
}
