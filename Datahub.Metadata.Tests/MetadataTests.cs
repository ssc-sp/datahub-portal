using System;
using Xunit;

namespace Datahub.Metadata.Tests
{
    public class MetadataTests
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
        public void Add_FieldWithDuplicatedId_ShouldThrow()
        {
            MetadataDefinition definitions = new();

            FieldDefinition field1 = new() { Id = "field_one" };
            definitions.Add(field1);

            FieldDefinition field2 = new() { Id = "field_one" };
            Assert.Throws<ArgumentException>(() => definitions.Add(field2));
        }

        [Fact]
        public void Get_WithExistingId_ShouldReturnExpected()
        {
            MetadataDefinition definitions = new();
            var fieldId = "known_id";
            FieldDefinition field = new() { Id = fieldId };
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
            FieldDefinition field = new() { Id = fieldId };
            definitions.Add(field);

            var actual = definitions.Get("unknow_id");

            Assert.Null(actual);
        }
    }
}
