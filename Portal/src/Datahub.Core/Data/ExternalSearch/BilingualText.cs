namespace Datahub.Core.Data.ExternalSearch;

public class BilingualText
{
    public required string En { get; set; }
    public required string Fr { get; set; }

    public string GetString(bool isFrench)
    {
        return isFrench ? Fr : En;
    }
}