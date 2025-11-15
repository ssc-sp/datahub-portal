namespace Datahub.Shared.Entities;

public class TerraformOutputVariable
{
    public bool Sensitive { get; set; }
    public required string Type { get; set; }
    public required string Value { get; set; }
}