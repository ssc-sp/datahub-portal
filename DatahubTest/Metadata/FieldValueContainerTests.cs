using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System.Linq;
using Xunit;

/// <summary>
/// Cannot name it 'Datahub.Tests.Metadata' or will conflict with an existing class
/// </summary>
namespace Datahub.Tests.Meta_Data
{
    public class FieldValueContainerTests
    {
        [Fact]
        public void FieldValueContainer_ValidateRequired_MustSucceedWhenAllRequiredFieldsEntered()
        {
            var definitions = GetDefinitions(("name", true), ("ignored", false), ("expected", true));
            var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

            var actual = container.ValidateRequired(f => f.Required_FLAG);

            Assert.True(actual);
        }

        [Fact]
        public void FieldValueContainer_ValidateRequired_MustFailWhenARequiredFieldsIsNotEntered()
        {
            var definitions = GetDefinitions(("name", true), ("not_ignored", true), ("expected", true));
            var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

            var actual = container.ValidateRequired(f => f.Required_FLAG);

            Assert.False(actual);
        }

        [Fact]
        public void FieldValueContainer_GetIndexableValues_MustReturnExpected()
        {
            var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

            var container = GetFieldValueContainer(fieldDefinitions,
                ("title_translated_en", "Sample file"),
                ("title_translated_fr", "Exemple de fichier"),
                ("topic_category", "environment"));

            var expected = "Sample file|Environment";
            var actual = container.GetIndexableValues("|", true, "title_translated_en", "topic_category");
            Assert.Equal(actual, expected);

            expected = "Exemple de fichier|Environnement";
            actual = container.GetIndexableValues("|", false, "title_translated_fr", "topic_category");
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void FieldValueContainer_GetValues_MustReturnExpected()
        {
            var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

            var container = GetFieldValueContainer(fieldDefinitions,
                ("title_translated_en", "Sample file"),
                ("title_translated_fr", "Exemple de fichier"),
                ("topic_category", "environment"));

            var actual = container.GetValues(true, "title_translated_fr", "title_translated_en", "topic_category");

            var expected = "Sample file";
            Assert.Equal(actual["title_translated_en"], expected);

            expected = "Environment";
            Assert.Equal(actual["topic_category"], expected);
        }

        [Fact]
        public void FieldValueContainer_GetValue_MustReturnExpected()
        {
            var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

            var container = GetFieldValueContainer(fieldDefinitions,
                ("title_translated_en", "Sample file"),
                ("title_translated_fr", "Exemple de fichier"),
                ("topic_category", "environment"));

            var expected = "Sample file";
            var actual = container["title_translated_en"].Value_TXT;
            Assert.Equal(actual, expected);

            expected = "Expected name";
            container.SetValue("name", expected);
            actual = container["name"].Value_TXT;
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void FieldValueContainer_GetSelectedChoices_MustReturnExpected()
        {
            var fieldDefinitions = FieldDefinitionHelper.LoadDefinitions();

            var container = GetFieldValueContainer(fieldDefinitions,
                ("title_translated_en", "Sample file"),
                ("title_translated_fr", "Exemple de fichier"),
                ("subject", "agriculture|law"));

            var choices = container.GetSelectedChoices("subject").ToArray();
            Assert.True(choices.Length == 2);

            var expected = "agriculture"; 
            var actual = choices[0].Value_TXT;
            Assert.Equal(actual, expected);

            expected = "law";
            actual = choices[1].Value_TXT;
            Assert.Equal(actual, expected);
        }


        static FieldDefinitions GetDefinitions(params (string name, bool required)[] fields)
        {
            FieldDefinitions definitions = new();
            var id = 1;
            definitions.Add(fields.Select(f => new FieldDefinition()
            {
                FieldDefinitionId = id++,
                Field_Name_TXT = f.name,
                Required_FLAG = f.required,
                English_DESC = f.name,
                French_DESC = f.name                
            }));
            return definitions;
        }

        static FieldValueContainer GetFieldValueContainer(FieldDefinitions definitions, params (string name, string value)[] fieldValues)
        {
            return new FieldValueContainer("1", definitions, fieldValues.Select(fv => new ObjectFieldValue() 
            { 
                FieldDefinition = definitions.Get(fv.name),
                FieldDefinitionId = definitions.Get(fv.name).FieldDefinitionId, 
                Value_TXT = fv.value 
            }));
        }
    }
}
