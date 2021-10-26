using Datahub.Metadata.Model;
using Datahub.Portal.Components.Metadata;
using Xunit;

/// <summary>
/// Cannot name it 'Datahub.Tests.Metadata' or will conflict with an existing class
/// </summary>
namespace Datahub.Tests.Meta_Data
{
    public class TranslatedFieldPairsTests
    {
        [Fact]
        public void BindField_ForNotTranslatedField_ShouldReturnNull()
        {
            // translated fields end in: _en or _fr nothing else is valid
            TranslatedFieldPairs translatedFieldPairs = new();
            Assert.Null(translatedFieldPairs.BindField("name", CreateFieldValue(1, "anyvalue")));
            Assert.Null(translatedFieldPairs.BindField("name_es", CreateFieldValue(2, "anyvalue")));
            Assert.Null(translatedFieldPairs.BindField("name_other", CreateFieldValue(3, "anyvalue")));
        }

        [Fact]
        public void BindField_ForTranslatedField_ShouldReturnNonNull()
        {
            TranslatedFieldPairs translatedFieldPairs = new();
            Assert.NotNull(translatedFieldPairs.BindField("name_en", CreateFieldValue(1, "anyvalue")));
            Assert.NotNull(translatedFieldPairs.BindField("name_fr", CreateFieldValue(2, "anyvalue")));
        }

        [Fact]
        public void BindField_SameRootField_ShouldReturnSameInstance()
        {
            string rootName = "Expected";

            ObjectFieldValue fieldValueEn = CreateFieldValue(1, "anyvalue");
            ObjectFieldValue fieldValueFr = CreateFieldValue(2, "anyvalue");

            TranslatedFieldPairs translatedFieldPairs = new();

            TranslatedFieldPair expected = translatedFieldPairs.BindField($"{rootName}_en", fieldValueEn);
            TranslatedFieldPair actual = translatedFieldPairs.BindField($"{rootName}_fr", fieldValueFr);

            Assert.NotNull(actual);
            Assert.Equal(actual, expected);
        }

        [Fact]
        public void BindField_SameRootField_ShouldAssignLanguagesProperly()
        {
            string rootName = "Expected";

            ObjectFieldValue fieldValueEn = CreateFieldValue(1, "anyvalue");
            ObjectFieldValue fieldValueFr = CreateFieldValue(2, "anyvalue");

            TranslatedFieldPairs translatedFieldPairs = new();

            _ = translatedFieldPairs.BindField($"{rootName}_en", fieldValueEn);
            TranslatedFieldPair actual = translatedFieldPairs.BindField($"{rootName}_fr", fieldValueFr);

            Assert.Equal(actual.FieldEnglish, fieldValueEn);
            Assert.Equal(actual.FieldFrench, fieldValueFr);
        }

        [Fact]
        public void BindField_DifferentRootField_ShouldReturnDifferentInstance()
        {
            ObjectFieldValue fieldValueEn = CreateFieldValue(1, "anyvalue");
            ObjectFieldValue fieldValueFr = CreateFieldValue(2, "anyvalue");

            TranslatedFieldPairs translatedFieldPairs = new();

            TranslatedFieldPair pair1 = translatedFieldPairs.BindField("rootName1_en", fieldValueEn);
            TranslatedFieldPair pair2 = translatedFieldPairs.BindField("rootName2_fr", fieldValueFr);

            Assert.NotEqual(pair1, pair2);
        }

        [Fact]
        public void GetPaired_SameRootField_ShouldReturnExpected()
        {
            ObjectFieldValue fieldValueEn = CreateFieldValue(1, "anyvalue");
            ObjectFieldValue fieldValueFr = CreateFieldValue(2, "anyvalue");

            TranslatedFieldPair translatedFieldPairs = new()
            {
                RootName = "rootName",
                FieldEnglish = fieldValueEn,
                FieldFrench = fieldValueFr
            };

            var expected = fieldValueFr;
            var actual = translatedFieldPairs.GetPaired(fieldValueEn);

            Assert.Equal(actual, expected);

            expected = fieldValueEn;
            actual = translatedFieldPairs.GetPaired(fieldValueFr);

            Assert.Equal(actual, expected);
        }

        static ObjectFieldValue CreateFieldValue(int definitionId, string value)
        {
            return new() { FieldDefinitionId = definitionId, Value_TXT = value };
        }
    }
}
