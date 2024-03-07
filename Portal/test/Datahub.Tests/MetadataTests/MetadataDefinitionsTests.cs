using System;
using System.Linq;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Xunit;

/// <summary>
/// Cannot name it 'Datahub.Tests.Metadata' or will conflict with an existing class
/// </summary>
namespace Datahub.Tests.MetadataTests;

public class MetadataDefinitionsTests
{
    [Fact]
    public void AddNullFieldShouldThrow()
    {
        FieldDefinitions definitions = new();
        FieldDefinition field = null!;
        Assert.Throws<ArgumentException>(() => definitions.Add(field));
    }

    [Fact]
    public void AddFieldWithEmptyIdShouldThrow()
    {
        FieldDefinitions definitions = new();
        FieldDefinition field = new();
        Assert.Throws<ArgumentException>(() => definitions.Add(field));
    }

    [Fact]
    public void AddFieldWithDuplicatedIdShouldSucceedWhenIgnoringDuplicates()
    {
        FieldDefinitions definitions = new(ignoreDuplicates: true);

        FieldDefinition field1 = new() { Field_Name_TXT = "field_one" };
        definitions.Add(field1);

        FieldDefinition field2 = new() { Field_Name_TXT = "field_one" };
        definitions.Add(field2);
    }

    [Fact]
    public void AddFieldWithDuplicatedIdShouldThrowWhenNotIgnoringDuplicates()
    {
        FieldDefinitions definitions = new(ignoreDuplicates: false);

        FieldDefinition field1 = new() { Field_Name_TXT = "field_one" };
        definitions.Add(field1);

        FieldDefinition field2 = new() { Field_Name_TXT = "field_one" };
        Assert.Throws<ArgumentException>(() => definitions.Add(field2));
    }

    [Fact]
    public void GetWithExistingIdShouldReturnExpected()
    {
        FieldDefinitions definitions = new();
        var fieldId = "known_id";
        FieldDefinition field = new() { Field_Name_TXT = fieldId };
        definitions.Add(field);

        var expected = field;
        var actual = definitions.Get(fieldId);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GetWithNonExistingIdShouldReturnNull()
    {
        FieldDefinitions definitions = new();
        var fieldId = "known_id";
        FieldDefinition field = new() { Field_Name_TXT = fieldId };
        definitions.Add(field);

        var actual = definitions.Get("unknow_id");

        Assert.Null(actual);
    }

    [Fact]
    public void GetFieldWithWithMultipleFieldsShouldReturnExpectedCountAndOrder()
    {
        FieldDefinitions definitions = new();
        FieldDefinition field1 = new() { Field_Name_TXT = "F1", Sort_Order_NUM = 2 };
        FieldDefinition field2 = new() { Field_Name_TXT = "F2", Sort_Order_NUM = 3 };
        FieldDefinition field3 = new() { Field_Name_TXT = "F3", Sort_Order_NUM = 1 };
        definitions.Add(field1);
        definitions.Add(field2);
        definitions.Add(field3);

        var fields = definitions.Fields.ToList();
        var expected = 3;

        Assert.Equal(expected, fields.Count);
        Assert.Equal(fields[0], field3);
        Assert.Equal(fields[1], field1);
        Assert.Equal(fields[2], field2);
    }

    [Fact]
    public void MetadataVersionWithMultipleFieldsShouldReturnExpectedValue()
    {
        var expected = 1;

        FieldDefinitions definitions = new();
        FieldDefinition field1 = new() { MetadataVersionId = expected, Field_Name_TXT = "F1", Sort_Order_NUM = 2 };
        FieldDefinition field2 = new() { MetadataVersionId = expected, Field_Name_TXT = "F2", Sort_Order_NUM = 3 };
        FieldDefinition field3 = new() { MetadataVersionId = expected, Field_Name_TXT = "F3", Sort_Order_NUM = 1 };
        definitions.Add(field1);
        definitions.Add(field2);
        definitions.Add(field3);

        var actual = definitions.MetadataVersion;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AddFieldWithMultipleMetadatVersionsShouldThrow()
    {
        Assert.Throws<ArgumentException>(() =>
        {
            var versionId = 1;
            var otherVersionId = 2;

            FieldDefinitions definitions = new();

            FieldDefinition field1 = new() { MetadataVersionId = versionId, Field_Name_TXT = "F1", Sort_Order_NUM = 2 };
            FieldDefinition field2 = new() { MetadataVersionId = versionId, Field_Name_TXT = "F2", Sort_Order_NUM = 3 };
            FieldDefinition field3 = new() { MetadataVersionId = otherVersionId, Field_Name_TXT = "F3", Sort_Order_NUM = 1 };

            definitions.Add(field1);
            definitions.Add(field2);
            definitions.Add(field3);
        });
    }
}