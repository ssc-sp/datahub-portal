using NRCan.Datahub.Metadata.DTO;
using NRCan.Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace NRCan.Datahub.Metadata.Tests
{
    public class MetadataDefinitionsTests
    {
        [Fact]
        public void Add_NullField_ShouldThrow()
        {
            FieldDefinitions definitions = new();
            FieldDefinition field = null;
            Assert.Throws<ArgumentException>(() => definitions.Add(field));
        }

        [Fact]
        public void Add_FieldWithEmptyId_ShouldThrow()
        {
            FieldDefinitions definitions = new();
            FieldDefinition field = new();
            Assert.Throws<ArgumentException>(() => definitions.Add(field));
        }

        [Fact]
        public void Add_FieldWithDuplicatedId_ShouldSucceedWhenIgnoringDuplicates()
        {
            FieldDefinitions definitions = new(ignoreDuplicates: true);

            FieldDefinition field1 = new() { Field_Name_TXT = "field_one" };
            definitions.Add(field1);

            FieldDefinition field2 = new() { Field_Name_TXT = "field_one" };
            definitions.Add(field2);
        }

        [Fact]
        public void Add_FieldWithDuplicatedId_ShouldThrowWhenNotIgnoringDuplicates()
        {
            FieldDefinitions definitions = new(ignoreDuplicates: false);

            FieldDefinition field1 = new() { Field_Name_TXT = "field_one" };
            definitions.Add(field1);

            FieldDefinition field2 = new() { Field_Name_TXT = "field_one" };
            Assert.Throws<ArgumentException>(() => definitions.Add(field2));
        }

        [Fact]
        public void Get_WithExistingId_ShouldReturnExpected()
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
        public void Get_WithNonExistingId_ShouldReturnNull()
        {
            FieldDefinitions definitions = new();
            var fieldId = "known_id";
            FieldDefinition field = new() { Field_Name_TXT = fieldId };
            definitions.Add(field);

            var actual = definitions.Get("unknow_id");

            Assert.Null(actual);
        }

        [Fact]
        public void GetField_WithWithMultipleFields_ShouldReturnExpectedCountAndOrder()
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
        public void MetadataVersion_WithMultipleFields_ShouldReturnExpectedValue()
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
        public void AddField_WithMultipleMetadatVersions_ShouldThrow()
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


        [Fact]
        public void ListTest()
        {
            List<string> list = new List<string>();
            list.Add("TEST");


            list = list.Select(x => $"{x}-admin").ToList();
            

            Assert.True(list[0] == "TEST-admin");
        }

    }
}
