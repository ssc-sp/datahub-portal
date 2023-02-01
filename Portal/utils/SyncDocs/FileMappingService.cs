using System.Text.Json;

namespace SyncDocs;

internal class FileMappingService
{
    private readonly List<FilePair> _pairs = new List<FilePair>();
    private readonly string _filePath;


    public FileMappingService(string filePath)
    {
        _filePath = filePath;
    }

	public void AddPair(string id, string engPath, string frePath)
	{
		_pairs.Add(new(id, engPath, frePath));
	}

    public bool CleanUpMappings()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var directory = Path.GetDirectoryName(_filePath)!;
                var curMappings = JsonSerializer.Deserialize<FilePair[]>(File.ReadAllText(_filePath)) ?? Array.Empty<FilePair>();
                foreach (var mapping in curMappings)
                {
                    var enPath = NormalizePath($"{directory}{mapping.En}");
                    if (!File.Exists(enPath))
                    {
                        var frPath = NormalizePath($"{directory}{mapping.Fr}");
                        File.Delete(frPath);
                    }
                }
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool SaveMappings()
	{
        try
        {
            var options = new JsonSerializerOptions() 
            { 
                WriteIndented = true 
            };

            // save new mapping
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_pairs, options));

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    static string NormalizePath(string path) => path.Replace("\\", "/");
}

record FilePair(string Id, string En, string Fr);
