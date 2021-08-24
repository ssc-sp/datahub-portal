namespace NRCan.Datahub.Metadata.Model
{
    public class ObjectFieldValue
    {
        public long ObjectMetadataId { get; set; }
        public int FieldDefinitionId { get; set; }
        public string Value_TXT { get; set; }
        public virtual ObjectMetadata ObjectMetadata { get; set; }
        public virtual FieldDefinition FieldDefinition { get; set; }

        public ObjectFieldValue Clone() => MemberwiseClone() as ObjectFieldValue;
    }
}
