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
            File.WriteAllText(path, JsonSerializer.Serialize(_pairs, options));
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

record FilePair(string Id, string En, string Fr);
