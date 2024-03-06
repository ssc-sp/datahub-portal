using Datahub.Core.Utils;
using System;
using System.Linq.Expressions;
using Datahub.Core.Model.Datahub;
using Datahub.Finance.Data;
using Xunit;

namespace Datahub.Tests.Forms;

public class FieldCodeGeneratorTests
{
    [Fact]
    public void GetFormattedFieldNameFieldWithValidCharsProducesExpectedName()
    {
        var expected = "CleanName";
        WebFormField field = new()
        {
            FieldDESC = "CleanName",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GetFormattedFieldName(field);

        Assert.Equal(actual, expected);
    }

    [Fact]
    public void GetFormattedFieldNameFieldWithInvalidCharsProducesExpectedName()
    {
        var expected = "Clean_Name";
        WebFormField field = new()
        {
            FieldDESC = "Clean !@#$%^&*()-Name",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GetFormattedFieldName(field);

        Assert.Equal(actual, expected);
    }

    [Fact]
    public void GenerateSQLNameForFieldWithExtensionProducesExpectedSqlName()
    {
        var expected = "Name_EXT";
        WebFormField field = new()
        {
            FieldDESC = "Name",
            ExtensionCD = "EXT"
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GenerateSQLName(field);

        Assert.Equal(actual, expected);
    }

    [Fact]
    public void GenerateSQLNameForFieldWithNoExtensionProducesExpectedJSON()
    {
        var expected = "Name";
        WebFormField field = new()
        {
            FieldDESC = "Name",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GenerateSQLName(field);

        Assert.Equal(actual, expected);
    }

    [Fact]
    public void GenerateJSONForFieldWithNoSpacesProducesExpectedJSON()
    {
        var fieldName = "ExpectedName";
        var expected = "\"ExpectedName\": \"ExpectedName\"";
        WebFormField field = new()
        {
            FieldDESC = fieldName
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GenerateJSON(field);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GenerateJSONForFieldWithSpaceProducesExpectedJSON()
    {
        var fieldName = "Field with spaces";
        var expected = $"\"Field_with_spaces\": \"{fieldName}\"";
        WebFormField field = new()
        {
            FieldDESC = fieldName
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var actual = generator.GenerateJSON(field);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void GenerateCSharpForRequiredFieldProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            MandatoryFLAG = true
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[Required]", csharp);
    }

    [Fact]
    public void GenerateCSharpForNotRequiredFieldProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            MandatoryFLAG = false
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.DoesNotContain("[Required]", csharp);
    }

    [Fact]
    public void GenerateCSharpForFieldWithMaxLengthProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            MaxLengthNUM = 32
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[MaxLength(32)]", csharp);
    }

    [Fact]
    public void GenerateCSharpForFieldWithSectionProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            SectionDESC = "AnySection"
        };

        var generator = new FieldCodeGenerator(s => 10);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[AeFormCategory(\"AnySection\", 10)]", csharp);
    }

    [Fact(Skip = "Needs to be validated")]
    public void GenerateCSharpForFieldTypeDropdawnProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            TypeCD = "Dropdown",
            DescriptionDESC = "Expected description",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[AeLabel(isDropDown: true, placeholder: \"[Expected description]\"]", csharp);
    }

    [Fact]
    public void GenerateCSharpForFieldTypeDropdawnWithChoicesProducesExpectedCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            TypeCD = "Dropdown",
            ChoicesTXT = "Red | Green | Blue | Light brown"
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[AeLabel(isDropDown: true, validValues: new [] { \"Red\", \"Green\", \"Blue\", \"Light brown\" })]", csharp);
    }

    [Fact]
    public void GenerateCSharpForFieldTypeMoneyProducesExpectedAnotationCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "AnyField",
            TypeCD = "Money",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[Column(TypeName=\"Money\")]", csharp);
    }

    [Fact]
    public void GenerateCSharpForFieldOfTypeProducesExpectedDeclarationCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "IntField",
            TypeCD = "Integer",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("public int IntField { get; set; }", csharp);
    }

    [Fact]
    public void GenerateCSharpForMoneyTypeFieldsProducesExpectedAnotationCode()
    {
        WebFormField field = new()
        {
            FieldDESC = "MoneyField",
            TypeCD = "Money",
        };

        var generator = new FieldCodeGenerator(DummyMap);
        var csharp = generator.GenerateCSharp(field);

        Assert.Contains("[Column(TypeName=\"Money\")]", csharp);
    }

    [Fact]
    public void ExpressionTest()
    {

        var parameterExpression = Expression.Parameter(typeof(HierarchyLevel), "fc");
        var prop = Expression.Property(parameterExpression, "FundCenterModifiedEnglish");
        var lambda = Expression.Lambda<Func<HierarchyLevel, string>>(prop, parameterExpression);
    }

    static int DummyMap(string section) => 1;
}