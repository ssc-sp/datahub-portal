using System.Text.Json;

namespace SyncDocs;

internal class FileMappingService
{
    private readonly List<FilePair> _pairs = new List<FilePair>();

	public void AddPair(string id, string engPath, string frePath)
	{
		_pairs.Add(new(id, engPath, frePath));
	}

	public bool SaveTo(string path)
	{
        try
        {
            var options = new JsonSerializerOptions() { WriteIndented = true };
            var directory = Path.GetDirectoryName(path)!;

            // delete unnecessary files
            if (File.Exists(path))
            {
                var oldMappings = JsonSerializer.Deserialize<FilePair[]>(File.ReadAllText(path)) ?? Array.Empty<FilePair>();
                foreach (var mapping in oldMappings)
                {
                    var enPath = NormalizePath($"{directory}{mapping.En}");
                    if (!File.Exists(enPath))
                    {
                        var frPath = NormalizePath($"{directory}{mapping.Fr}");
                        File.Delete(frPath);
                    }
                }
            }

            // save new mapping
            File.WriteAllText(path, JsonSerializer.Serialize(_pairs, options));

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
