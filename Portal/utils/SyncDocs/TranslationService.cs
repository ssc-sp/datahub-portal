using DeepL;
namespace SyncDocs;

internal class TranslationService
{
    private readonly Translator? _client;
    private readonly string _sourcePath;

    public TranslationService(string sourcePath, string deeplKey) 
    {
        _sourcePath = sourcePath;
        _client = !string.IsNullOrEmpty(deeplKey) ? new Translator(deeplKey) : null;
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

    public async Task TranslateMarkupFile(string sourcePath, string outputPath, bool isSidebar)
    {
        if (_client is null)
            return;

        var sourceFileUrl = GetSourceFilePathUrl(sourcePath);
        var remarks = $"[_metadata_: remarks]:- \"Automatically translated with DeepL. From: {sourceFileUrl}\"";
        var note = $"[_(draft documentation, please review)_]({sourceFileUrl})";

        using var writer = new StreamWriter(outputPath);

        writer.WriteLine(remarks);
        writer.WriteLine();

        writer.WriteLine(note);
        writer.WriteLine();

        foreach (var line in File.ReadAllLines(sourcePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                writer.WriteLine();
                continue;
            }
            var translated = isSidebar ? await TranslateSidebar(line, "/fr") : await Translate(line);
            writer.WriteLine(translated);
        }
    }

    private async Task<string> TranslateSidebar(string text, string relPath)
    {
        var link = TryMatchLink(text);
        if (link == null)
        {
            return await Translate(text);
        }
        else
        {
            var heading = await Translate(link.Heading);
            var path = await TranslateFilePath(link.Path);
            return string.Format(link.Template, heading, $"{relPath}{path}");
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
        var translateResult = await _client!.TranslateTextAsync(text, LanguageCode.English, LanguageCode.French);
        return translateResult?.Text ?? text;
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

    static LinkData? TryMatchLink(string text)
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
