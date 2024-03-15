namespace Datahub.Shared.Entities;

public class TerraformOutputVariable
{
    public bool Sensitive { get; set; }
    public string Type { get; set; }
    public string Value { get; set; }
}