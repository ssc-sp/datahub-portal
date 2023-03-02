using System.Text.Json;

namespace SyncDocs;

internal class DictionaryCache
{
    private readonly Dictionary<string, NamePair> _cache;
    private readonly string _path;
    private bool _changed;

    public DictionaryCache(string path)
    {
        _path = path;
        _cache = LoadCache(path).ToDictionary(kv => kv.English, kv => kv);
    }

    public string? GetFrenchTranslation(string key)
    {
        return _cache.TryGetValue(key, out NamePair? pair) ? pair?.French : default;
    }

    public void SaveFrenchTranslation(string english, string french)
    {
        if (!_cache.ContainsKey(english))
        {
            _cache[english] = new(english, french);
            _changed = true;
        }
    }

    public bool SaveChanges()
    {
        if (_changed)
        {
            try
            {
                var options = new JsonSerializerOptions() { WriteIndented = true };
                File.WriteAllText(_path, JsonSerializer.Serialize(_cache.Values.ToList(), options));
            }
            catch (Exception)
            {
                return false;
            }    
        }
        return true;
    }

    private static IEnumerable<NamePair> LoadCache(string path)
    {
        var result = Array.Empty<NamePair>();
        if (File.Exists(path))
        {
            try
            {
                return JsonSerializer.Deserialize<NamePair[]>(File.ReadAllText(path)) ?? result;
            }
            catch
            {
            }
        }
        return result;
    }
}

record NamePair(string English, string French);
