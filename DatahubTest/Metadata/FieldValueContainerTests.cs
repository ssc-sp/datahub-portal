using NRCan.Datahub.Metadata.DTO;
using NRCan.Datahub.Metadata.Model;
using System.Linq;
using Xunit;

namespace DatahubTest
{
    public class FieldValueContainerTests
    {
        [Fact]
        public void FieldValueContainer_ValidateRequired_MustSucceedWhenAllRequiredFieldsEntered()
        {
            var definitions = GetDefinitions(("name", true), ("ignored", false), ("expected", true));
            var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

            var actual = container.ValidateRequired();

            Assert.True(actual);
        }

        [Fact]
        public void FieldValueContainer_ValidateRequired_MustFailWhenARequiredFieldsIsNotEntered()
        {
            var definitions = GetDefinitions(("name", true), ("not_ignored", true), ("expected", true));
            var container = GetFieldValueContainer(definitions, ("name", "any name"), ("expected", "any value"));

            var actual = container.ValidateRequired();

            Assert.False(actual);
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
                FieldDefinitionId = definitions.Get(fv.name).FieldDefinitionId, 
                Value_TXT = fv.value 
            }));
        }
    }
}
