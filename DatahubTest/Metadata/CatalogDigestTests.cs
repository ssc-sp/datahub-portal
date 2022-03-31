using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Datahub.Metadata.Utils;

namespace Datahub.Tests.Meta_Data
{
    public class CatalogDigestTests
    {

        [Fact]
        public void CreateCalatogDigest_ShouldReturnExpected()
        {
            FieldValueContainer fieldValues = new(1, "id1", GetFieldDefinitions(), new List<ObjectFieldValue>());
            fieldValues.SetValue("name", "Name1");
            fieldValues.SetValue("contact_information", "user1@email.com");
            fieldValues.SetValue("sector", "25");
            fieldValues.SetValue("branch", "125");
            fieldValues.SetValue("subject", "subject1");
            fieldValues.SetValue("programs", "P1");
            fieldValues.SetValue("keywords_en", "KE1,KE2");
            fieldValues.SetValue("keywords_fr", "KF1,KF2");

            var actual = fieldValues.GetCatalogDigest();

            var expected = "ke1 ke2 subject1 p1 sector_en1 branch_en1 name1";
            Assert.Equal(actual.EnglishCatalog, expected);

            expected = "kf1 kf2 subject1 p1 sector_fr1 branch_fr1 name1";
            Assert.Equal(actual.FrenchCatalog, expected);

            expected = "Name1";
            Assert.Equal(actual.Name, expected);

            expected = "user1@email.com";
            Assert.Equal(actual.Contact, expected);

            Assert.True(actual.Sector == 25);
            Assert.True(actual.Branch == 125);
        }

        static FieldDefinitions GetFieldDefinitions()
        {
            FieldDefinitions definitions = new FieldDefinitions();
            definitions.Add(MakeDefinition(1, "name"));
            definitions.Add(MakeDefinition(2, "contact_information"));
            definitions.Add(MakeDefinition(3, "sector", MakeChoice(3, "25", "sector_en1", "sector_fr1")));
            definitions.Add(MakeDefinition(4, "branch", MakeChoice(4, "125", "branch_en1", "branch_fr1")));
            definitions.Add(MakeDefinition(5, "subject", MakeChoice(5, "subject1")));
            definitions.Add(MakeDefinition(6, "programs", MakeChoice(6, "P1")));
            definitions.Add(MakeDefinition(7, "keywords_en"));
            definitions.Add(MakeDefinition(8, "keywords_fr"));
            return definitions;
        }

        static FieldDefinition MakeDefinition(int id, string name, params FieldChoice[] choices)
        {
            return new()
            {
                FieldDefinitionId = id,
                Field_Name_TXT = name,
                Choices = choices.ToList()
            };
        }

        static FieldChoice MakeChoice(int defId, string value, string enLabel = null, string frLabel = null) => new() 
        { 
            FieldDefinitionId = defId, 
            Value_TXT = value,
            Label_English_TXT = enLabel ?? value,
            Label_French_TXT = frLabel ?? value
        };
    }
}
