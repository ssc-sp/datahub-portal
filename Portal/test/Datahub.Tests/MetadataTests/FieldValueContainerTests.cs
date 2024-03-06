﻿using System.Linq;
using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Xunit;

/// <summary>
/// Cannot name it 'Datahub.Tests.Metadata' or will conflict with an existing class
/// </summary>
namespace Datahub.Tests.MetadataTests;

public class FieldValueContainerTests
{
    [Fact]
    public void FieldValueContainerValidateRequiredMustSucceedWhenAllRequiredFieldsEntered()
    {
        var definitions = GetDefinitions(("name", true), ("ignored", false), ("expected", true));
        var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

        var actual = container.ValidateRequired(f => f.RequiredFLAG);

        Assert.True(actual);
    }

    [Fact]
    public void FieldValueContainerValidateRequiredMustFailWhenARequiredFieldsIsNotEntered()
    {
        var definitions = GetDefinitions(("name", true), ("not_ignored", true), ("expected", true));
        var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

        var actual = container.ValidateRequired(f => f.RequiredFLAG);

        Assert.False(actual);
    }

    [Fact]
    public void FieldValueContainerGetValueMustReturnExpected()
    {
        var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

        var container = GetFieldValueContainer(fieldDefinitions,
            ("title_translated_en", "Sample file"),
            ("title_translated_fr", "Exemple de fichier"),
            ("topic_category", "environment"));

        var expected = "Sample file";
        var actual = container["title_translated_en"].ValueTXT;
        Assert.Equal(actual, expected);

        expected = "Expected name";
        container.SetValue("name", expected);
        actual = container["name"].ValueTXT;
        Assert.Equal(actual, expected);
    }

    [Fact]
    public void FieldValueContainerGetSelectedChoicesMustReturnExpected()
    {
        var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

        var container = GetFieldValueContainer(fieldDefinitions,
            ("title_translated_en", "Sample file"),
            ("title_translated_fr", "Exemple de fichier"),
            ("subject", "agriculture|law"));

        var choices = container.GetSelectedChoices("subject").ToArray();
        Assert.True(choices.Length == 2);

        var expected = "agriculture";
        var actual = choices[0].ValueTXT;
        Assert.Equal(actual, expected);

        expected = "law";
        actual = choices[1].ValueTXT;
        Assert.Equal(actual, expected);
    }


    static FieldDefinitions GetDefinitions(params (string name, bool required)[] fields)
    {
        FieldDefinitions definitions = new();
        var id = 1;
        definitions.Add(fields.Select(f => new FieldDefinition()
        {
            FieldDefinitionId = id++,
            FieldNameTXT = f.name,
            RequiredFLAG = f.required,
            EnglishDESC = f.name,
            FrenchDESC = f.name
        }));
        return definitions;
    }

    static FieldValueContainer GetFieldValueContainer(FieldDefinitions definitions, params (string name, string value)[] fieldValues)
    {
        return new FieldValueContainer(1, "1", definitions, fieldValues.Select(fv => new ObjectFieldValue()
        {
            FieldDefinition = definitions.Get(fv.name),
            FieldDefinitionId = definitions.Get(fv.name).FieldDefinitionId,
            ValueTXT = fv.value
        }));
    }
}