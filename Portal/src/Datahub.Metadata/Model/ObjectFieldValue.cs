﻿namespace Datahub.Metadata.Model;

public class ObjectFieldValue
{
    public long ObjectMetadataId { get; set; }
    public int FieldDefinitionId { get; set; }
    public string ValueTXT { get; set; }
    public virtual ObjectMetadata ObjectMetadata { get; set; }
    public virtual FieldDefinition FieldDefinition { get; set; }

    public ObjectFieldValue Clone() => MemberwiseClone() as ObjectFieldValue;

    public override string ToString()
    {
        var fielName = FieldDefinition?.Name ?? FieldDefinitionId.ToString();
        return $"{fielName} : '{ValueTXT}'";
    }
}