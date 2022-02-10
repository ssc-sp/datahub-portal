namespace Datahub.Metadata.Model
{
    public class SectionField
    {
        public int SectionId { get; set; }
        public int FieldDefinitionId { get; set; }
        public bool Required_FLAG { get; set; }
        public virtual MetadataSection Section { get; set; }
        public virtual FieldDefinition FieldDefinition { get; set; }
    }
}
