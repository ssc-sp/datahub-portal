using System;
using System.Linq;
using Xunit;

namespace NRCan.Datahub.Metadata.Tests
{
    public class MetadataDefinitionTests
    {
        [Fact]
        public void Add_NullField_ShouldThrow()
        {
            MetadataDefinition definitions = new();
            FieldDefinition field = null;
            Assert.Throws<ArgumentException>(() => definitions.Add(field));
        }

        [Fact]
        public void Add_FieldWithEmptyId_ShouldThrow()
        {
            MetadataDefinition definitions = new();
            FieldDefinition field = new();
            Assert.Throws<ArgumentException>(() => definitions.Add(field));
        }

        [Fact]
        public void Add_FieldWithDuplicatedId_ShouldSucceedWhenIgnoringDuplicates()
        {
            MetadataDefinition definitions = new(ignoreDuplicates: true);

            FieldDefinition field1 = new() { FieldName = "field_one" };
            definitions.Add(field1);

            FieldDefinition field2 = new() { FieldName = "field_one" };
            definitions.Add(field2);
        }

        [Fact]
        public void Add_FieldWithDuplicatedId_ShouldThrowWhenNotIgnoringDuplicates()
        {
            MetadataDefinition definitions = new(ignoreDuplicates: false);

            FieldDefinition field1 = new() { FieldName = "field_one" };
            definitions.Add(field1);

            FieldDefinition field2 = new() { FieldName = "field_one" };
            Assert.Throws<ArgumentException>(() => definitions.Add(field2));
        }

        [Fact]
        public void Get_WithExistingId_ShouldReturnExpected()
        {
            MetadataDefinition definitions = new();
            var fieldId = "known_id";
            FieldDefinition field = new() { FieldName = fieldId };
            definitions.Add(field);

            var expected = field;
            var actual = definitions.Get(fieldId);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Get_WithNonExistingId_ShouldReturnNull()
        {
            MetadataDefinition definitions = new();
            var fieldId = "known_id";
            FieldDefinition field = new() { FieldName = fieldId };
            definitions.Add(field);

            var actual = definitions.Get("unknow_id");

            Assert.Null(actual);
        }

        [Fact]
        public void GetField_WithWithMultipleFields_ShouldReturnExpectedCountAndOrder()
        {
            MetadataDefinition definitions = new();
            FieldDefinition field1 = new() { FieldName = "F1", SortOrder = 2 };
            FieldDefinition field2 = new() { FieldName = "F2", SortOrder = 3 };
            FieldDefinition field3 = new() { FieldName = "F3", SortOrder = 1 };
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
    }
}
