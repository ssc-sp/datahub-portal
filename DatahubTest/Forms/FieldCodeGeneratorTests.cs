using Datahub.Core.UserTracking;
using Datahub.Core.Utils;
using Xunit;

namespace Datahub.Tests.Forms
{
    public class FieldCodeGeneratorTests
    {
        [Fact]
        public void GetFormattedFieldName_FieldWithValidChars_ProducesExpectedName()
        {
            var expected = "CleanName";
            WebForm_Field field = new()
            {
                Field_DESC = "CleanName",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GetFormattedFieldName(field);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GetFormattedFieldName_FieldWithInvalidChars_ProducesExpectedName()
        {
            var expected = "Clean_Name";
            WebForm_Field field = new()
            {
                Field_DESC = "Clean !@#$%^&*()-Name",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GetFormattedFieldName(field);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GenerateSQLName_ForFieldWithExtension_ProducesExpectedSqlName()
        {
            var expected = "Name_EXT";
            WebForm_Field field = new()
            {
                Field_DESC = "Name",
                Extension_CD = "EXT"
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GenerateSQLName(field);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GenerateSQLName_ForFieldWithNoExtension_ProducesExpectedJSON()
        {
            var expected = "Name";
            WebForm_Field field = new()
            {
                Field_DESC = "Name",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GenerateSQLName(field);

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GenerateJSON_ForFieldWithNoSpaces_ProducesExpectedJSON()
        {
            var fieldName = "ExpectedName";
            var expected = "\"ExpectedName\": \"ExpectedName\"";
            WebForm_Field field = new()
            {
                Field_DESC = fieldName
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GenerateJSON(field);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GenerateJSON_ForFieldWithSpace_ProducesExpectedJSON()
        {
            var fieldName = "Field with spaces";
            var expected = $"\"Field_with_spaces\": \"{ fieldName }\"";
            WebForm_Field field = new()
            {
                Field_DESC = fieldName
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var actual = generator.GenerateJSON(field);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void GenerateCSharp_ForRequiredField_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Mandatory_FLAG = true
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[Required]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForNotRequiredField_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Mandatory_FLAG = false
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.DoesNotContain("[Required]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldWithMaxLength_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Max_Length_NUM = 32
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[MaxLength(32)]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldWithSection_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Section_DESC = "AnySection"
            };

            var generator = new FieldCodeGenerator(s => 10);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[AeFormCategory(\"AnySection\", 10)]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldTypeDropdawn_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Type_CD = "Dropdown",
                Description_DESC = "Expected description",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[AeLabel(isDropDown: true, placeholder: \"[Expected description]\"]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldTypeDropdawnWithChoices_ProducesExpectedCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Type_CD = "Dropdown",
                Choices_TXT = "Red | Green | Blue | Light brown"
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[AeLabel(isDropDown: true, validValues: new [] { \"Red\", \"Green\", \"Blue\", \"Light brown\" }]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldTypeMoney_ProducesExpectedAnotationCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "AnyField",
                Type_CD = "Money",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[Column(TypeName=\"Money\")]", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForFieldOfType_ProducesExpectedDeclarationCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "IntField",
                Type_CD = "Integer",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("public int IntField { get; set; }", csharp);
        }

        [Fact]
        public void GenerateCSharp_ForMoneyTypeFields_ProducesExpectedAnotationCode()
        {
            WebForm_Field field = new()
            {
                Field_DESC = "MoneyField",
                Type_CD = "Money",
            };

            var generator = new FieldCodeGenerator(DummyMap);
            var csharp = generator.GenerateCSharp(field);

            Assert.Contains("[Column(TypeName=\"Money\")]", csharp);
        }

        static int DummyMap(string section) => 1;
    }
}
