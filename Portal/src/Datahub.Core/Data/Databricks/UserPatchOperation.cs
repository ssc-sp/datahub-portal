namespace Datahub.Core.Data.Databricks;

public class UserPatchOperation
{
    public string Op { get; set; }
    public string Path { get; set; }
    public DatabricksUser Value { get; set; }
}
