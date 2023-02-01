using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.Metadata.DTO;

public class MetadataDTO
{
    public static MetadataDTO Create(IEnumerable<MetadataProfile> profiles, IEnumerable<FieldDefinition> fieldDefinitions) => new()
    {
        Profiles = profiles.Select(p => ProfileDTO.Create(p)).ToList(),
        Definitions = fieldDefinitions.Select(f => FieldDefinitionDTO.Create(f)).ToList()
    };

    public List<ProfileDTO> Profiles { get; set; }
    public List<FieldDefinitionDTO> Definitions { get; set; }
}

public class ProfileDTO
{
    public static ProfileDTO Create(MetadataProfile source) => new()
    {
        Name = source.Name,
        Sections = source.Sections.Select(s => SectionDTO.Create(s)).ToList()
    };

    public string Name { get; set; }
    public List<SectionDTO> Sections { get; set; }
}

public class SectionDTO
{
    public static SectionDTO Create(MetadataSection source) => new()
    {
        Name_English_TXT = source.Name_English_TXT,
        Name_French_TXT = source.Name_French_TXT,
        Fields = source.Fields.Select(f => SectionFieldDTO.Create(f)).ToList()
    };

    public string Name_English_TXT { get; set; }
    public string Name_French_TXT { get; set; }
    public List<SectionFieldDTO> Fields { get; set; }
}

public class SectionFieldDTO
{
    public static SectionFieldDTO Create(SectionField source) => new()
    {
        FieldDefinitionId = source.FieldDefinitionId,
        Required_FLAG = source.Required_FLAG
    };

    public int FieldDefinitionId { get; set; }
    public bool Required_FLAG { get; set; }
}

public class FieldDefinitionDTO
{
    public static FieldDefinitionDTO Create(FieldDefinition source) => new()
    {
        FieldDefinitionId = source.FieldDefinitionId,
        Field_Name_TXT = source.Field_Name_TXT,
        Sort_Order_NUM = source.Sort_Order_NUM,
        Name_English_TXT = source.Name_English_TXT,
        Name_French_TXT = source.Name_French_TXT,
        English_DESC = source.English_DESC,
        French_DESC = source.French_DESC,
        Required_FLAG = source.Required_FLAG,
        MultiSelect_FLAG = source.MultiSelect_FLAG,
        Validators_TXT = source.Validators_TXT,
        Custom_Field_FLAG = source.Custom_Field_FLAG,
        Default_Value_TXT = source.Default_Value_TXT,
        CascadeParentId = source.CascadeParentId,
        Choices = source.Choices.Select(c => FieldChoiceDTO.Create(c)).ToList()
    };

    public int FieldDefinitionId { get; set; }
    public string Field_Name_TXT { get; set; }
    public int Sort_Order_NUM { get; set; }
    public string Name_English_TXT { get; set; }
    public string Name_French_TXT { get; set; }
    public string English_DESC { get; set; }
    public string French_DESC { get; set; }
    public bool Required_FLAG { get; set; }
    public bool MultiSelect_FLAG { get; set; }
    public string Validators_TXT { get; set; }
    public bool Custom_Field_FLAG { get; set; }
    public string Default_Value_TXT { get; set; }
    public int? CascadeParentId { get; set; }
    public List<FieldChoiceDTO> Choices { get; set; }
}

public class FieldChoiceDTO
{
    public static FieldChoiceDTO Create(FieldChoice source) => new()
    {
        Value_TXT = source.Value_TXT,
        Cascading_Value_TXT = source.Cascading_Value_TXT,
        Label_English_TXT = source.Label_English_TXT,
        Label_French_TXT = source.Label_French_TXT
    };

    public string Value_TXT { get; set; }
    public string Cascading_Value_TXT { get; set; }
    public string Label_English_TXT { get; set; }
    public string Label_French_TXT { get; set; }
}

public static class MetadataDTOExtensions
{
    public static FieldDefinition ToEntity(this FieldDefinitionDTO source, int versionId)
    {
        var definition = new FieldDefinition()
        {
            MetadataVersionId = versionId,
            Field_Name_TXT = source.Field_Name_TXT,
            Sort_Order_NUM = source.Sort_Order_NUM,
            Name_English_TXT = source.Name_English_TXT,
            Name_French_TXT = source.Name_French_TXT,
            English_DESC = source.English_DESC,
            French_DESC = source.French_DESC,
            Required_FLAG = source.Required_FLAG,
            MultiSelect_FLAG = source.MultiSelect_FLAG,
            Validators_TXT = source.Validators_TXT,
            Custom_Field_FLAG = source.Custom_Field_FLAG,
            Default_Value_TXT = source.Default_Value_TXT,
            CascadeParentId = source.CascadeParentId,
            Choices = source.Choices.Select(c => c.ToEntity()).ToList()
        };
        return definition;
    }

    public static FieldChoice ToEntity(this FieldChoiceDTO source, int definitionId = 0)
    {
        return new()
        {
            FieldDefinitionId = definitionId,
            Value_TXT = source.Value_TXT,
            Cascading_Value_TXT = source.Cascading_Value_TXT,
            Label_English_TXT = source.Label_English_TXT,
            Label_French_TXT = source.Label_French_TXT
        };
    }

    public static MetadataProfile ToEntity(this ProfileDTO source, Func<int, int> idMapper)
    {
        var profile = new MetadataProfile()
        {
            Name = source.Name,
            Sections = source.Sections.Select(s => s.ToEntity(idMapper)).ToList()
        };
        return profile;
    }

    public static MetadataSection ToEntity(this SectionDTO source, Func<int, int> idMapper)
    {
        var section = new MetadataSection()
        {
            Name_English_TXT = source.Name_English_TXT,
            Name_French_TXT = source.Name_French_TXT,
            Fields = source.Fields.Select(f => new SectionField() 
            { 
                FieldDefinitionId = idMapper(f.FieldDefinitionId),
                Required_FLAG = f.Required_FLAG
            }).ToList()
        };
        return section;
    }


}
