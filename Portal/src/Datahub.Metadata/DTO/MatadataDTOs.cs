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
        NameEnglishTXT = source.NameEnglishTXT,
        NameFrenchTXT = source.NameFrenchTXT,
        Fields = source.Fields.Select(f => SectionFieldDTO.Create(f)).ToList()
    };

    public string NameEnglishTXT { get; set; }
    public string NameFrenchTXT { get; set; }
    public List<SectionFieldDTO> Fields { get; set; }
}

public class SectionFieldDTO
{
    public static SectionFieldDTO Create(SectionField source) => new()
    {
        FieldDefinitionId = source.FieldDefinitionId,
        RequiredFLAG = source.RequiredFLAG
    };

    public int FieldDefinitionId { get; set; }
    public bool RequiredFLAG { get; set; }
}

public class FieldDefinitionDTO
{
    public static FieldDefinitionDTO Create(FieldDefinition source) => new()
    {
        FieldDefinitionId = source.FieldDefinitionId,
        FieldNameTXT = source.FieldNameTXT,
        SortOrderNUM = source.SortOrderNUM,
        NameEnglishTXT = source.NameEnglishTXT,
        NameFrenchTXT = source.NameFrenchTXT,
        EnglishDESC = source.EnglishDESC,
        FrenchDESC = source.FrenchDESC,
        RequiredFLAG = source.RequiredFLAG,
        MultiSelectFLAG = source.MultiSelectFLAG,
        ValidatorsTXT = source.ValidatorsTXT,
        CustomFieldFLAG = source.CustomFieldFLAG,
        DefaultValueTXT = source.DefaultValueTXT,
        CascadeParentId = source.CascadeParentId,
        Choices = source.Choices.Select(c => FieldChoiceDTO.Create(c)).ToList()
    };

    public int FieldDefinitionId { get; set; }
    public string FieldNameTXT { get; set; }
    public int SortOrderNUM { get; set; }
    public string NameEnglishTXT { get; set; }
    public string NameFrenchTXT { get; set; }
    public string EnglishDESC { get; set; }
    public string FrenchDESC { get; set; }
    public bool RequiredFLAG { get; set; }
    public bool MultiSelectFLAG { get; set; }
    public string ValidatorsTXT { get; set; }
    public bool CustomFieldFLAG { get; set; }
    public string DefaultValueTXT { get; set; }
    public int? CascadeParentId { get; set; }
    public List<FieldChoiceDTO> Choices { get; set; }
}

public class FieldChoiceDTO
{
    public static FieldChoiceDTO Create(FieldChoice source) => new()
    {
        ValueTXT = source.ValueTXT,
        CascadingValueTXT = source.CascadingValueTXT,
        LabelEnglishTXT = source.LabelEnglishTXT,
        LabelFrenchTXT = source.LabelFrenchTXT
    };

    public string ValueTXT { get; set; }
    public string CascadingValueTXT { get; set; }
    public string LabelEnglishTXT { get; set; }
    public string LabelFrenchTXT { get; set; }
}

public static class MetadataDTOExtensions
{
    public static FieldDefinition ToEntity(this FieldDefinitionDTO source, int versionId)
    {
        var definition = new FieldDefinition()
        {
            MetadataVersionId = versionId,
            FieldNameTXT = source.FieldNameTXT,
            SortOrderNUM = source.SortOrderNUM,
            NameEnglishTXT = source.NameEnglishTXT,
            NameFrenchTXT = source.NameFrenchTXT,
            EnglishDESC = source.EnglishDESC,
            FrenchDESC = source.FrenchDESC,
            RequiredFLAG = source.RequiredFLAG,
            MultiSelectFLAG = source.MultiSelectFLAG,
            ValidatorsTXT = source.ValidatorsTXT,
            CustomFieldFLAG = source.CustomFieldFLAG,
            DefaultValueTXT = source.DefaultValueTXT,
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
            ValueTXT = source.ValueTXT,
            CascadingValueTXT = source.CascadingValueTXT,
            LabelEnglishTXT = source.LabelEnglishTXT,
            LabelFrenchTXT = source.LabelFrenchTXT
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
            NameEnglishTXT = source.NameEnglishTXT,
            NameFrenchTXT = source.NameFrenchTXT,
            Fields = source.Fields.Select(f => new SectionField()
            {
                FieldDefinitionId = idMapper(f.FieldDefinitionId),
                RequiredFLAG = f.RequiredFLAG
            }).ToList()
        };
        return section;
    }


}
