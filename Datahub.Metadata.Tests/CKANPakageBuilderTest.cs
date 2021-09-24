using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using CsvHelper.Configuration;
using System.Globalization;
using NRCan.Datahub.Metadata.Model;
using Newtonsoft.Json;
using NRCan.Datahub.Metadata.DTO;

namespace NRCan.Datahub.Metadata.Tests
{
    public class CKANPakageBuilderTest
    {
        [Fact]
        public void CKANPakageBuilder_GivenMetadata_MustConvertToExpectedJson()
        {
            var definitions = LoadDefinitions();
            Assert.NotNull(definitions);

            var fieldValues = LoadFields(definitions);
            Assert.NotNull(fieldValues);

            CKanPackageGenerator generator = new();

            var dict = generator.GeneratePackage(fieldValues);

            var json = JsonConvert.SerializeObject(dict);

            Assert.NotNull(dict);
        }

        static FieldDefinitions LoadDefinitions()
        {
            var defs = JsonConvert.DeserializeObject<FieldDefinition[]>(GetFileContent("open_data_definitions.json")).ToList();
            var choices = JsonConvert.DeserializeObject<FieldChoice[]>(GetFileContent("open_data_definition_choices.json"));

            var defDict = defs.ToDictionary(v => v.FieldDefinitionId, v => v);
            foreach (var c in choices)
            {
                var def = defDict[c.FieldDefinitionId];
                def.Choices ??= new List<FieldChoice>();
                def.Choices.Add(c);
            }

            FieldDefinitions fieldDefinitions = new();
            fieldDefinitions.Add(defs);

            return fieldDefinitions;
        }

        static FieldValueContainer LoadFields(FieldDefinitions definitions)
        {
            var fieldValues = JsonConvert.DeserializeObject<ObjectFieldValue[]>(GetFileContent("field_values.json")).ToList();
            
            foreach (var fvalue in fieldValues)
            {
                fvalue.FieldDefinition = definitions.Get(fvalue.FieldDefinitionId);
            }

            return new FieldValueContainer(fieldValues);
        }

        static string GetFileContent(string fileName) => File.ReadAllText($"./Data/{fileName}");
    }

    class CKanPackageGenerator
    {
        readonly List<FieldAgent> _fieldAgents;

        public CKanPackageGenerator()
        {
            _fieldAgents = new()
            {
                new KeywordFieldAgent(),
                new TranslatedFieldAgent(),
                new CatchAllFieldAgent(true),
                new CatchAllFieldAgent(false)
            }; 
        }

        public Dictionary<string, object> GeneratePackage(FieldValueContainer fieldValues)
        {
            Dictionary<string, object> dict = new();

            var agents = InstanciateAgents(fieldValues).ToList();
            foreach (var agent in agents)
            {
                agent.RenderField(dict);
            }

            return dict;
        }

        private IEnumerable<FieldAgent> InstanciateAgents(IEnumerable<ObjectFieldValue> fieldValues)
        {
            foreach (var fv in fieldValues)
            {
                var definition = fv.FieldDefinition;
                var matchingAgent = _fieldAgents.FirstOrDefault(a => a.Matches(definition));

                var tuple = matchingAgent.Instanciate(definition.Field_Name_TXT, fv.Value_TXT);
                if (tuple.append)
                {
                    yield return tuple.agent;
                }
            }
        }
    }

    abstract class FieldAgent
    {
        public abstract bool Matches(FieldDefinition definition);
        public abstract (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue);
        public abstract void RenderField(IDictionary<string, object> data);
    }

    class CatchAllFieldAgent : FieldAgent
    {
        readonly string _fieldName;
        readonly string _fieldValue;
        readonly bool _multiselect;

        public CatchAllFieldAgent(bool multiselect)
        {
            _multiselect = multiselect;
        }

        private CatchAllFieldAgent(string fieldName, string fieldValue, bool multiselect)
        {
            _fieldName = fieldName;
            _fieldValue = fieldValue;
            _multiselect = multiselect;
        }

        public override bool Matches(FieldDefinition definition) => _multiselect == definition.MultiSelect_FLAG; 

        public override (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue)
        {
            return (append: true, agent: new CatchAllFieldAgent(fieldName, fieldValue, _multiselect));
        }

        public override void RenderField(IDictionary<string, object> data)
        {
            data[_fieldName] = _multiselect ? _fieldValue.Split('|') : _fieldValue;
        }
    }

    class KeywordFieldAgent : FieldAgent
    {
        const string KeywordPrefix = "keywords_";

        readonly Dictionary<string, string[]> _languages = new();

        public override bool Matches(FieldDefinition definition) => definition.Field_Name_TXT.StartsWith(KeywordPrefix, StringComparison.InvariantCulture);

        public override (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue)
        {
            var append = _languages.Count == 0;

            var language = fieldName.Substring(KeywordPrefix.Length);
            _languages[language] = fieldValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            return (append, this);
        }

        public override void RenderField(IDictionary<string, object> data)
        {
            data["keywords"] = _languages;
        }
    }

    class TranslatedFieldAgent : FieldAgent
    {
        readonly Dictionary<string, Dictionary<string, string>> _fields = new();
        
        public override bool Matches(FieldDefinition definition)
        {
            var fieldName = definition.Field_Name_TXT;
            return fieldName.EndsWith("_en") || fieldName.EndsWith("_fr");
        }

        public override (bool append, FieldAgent agent) Instanciate(string fieldName, string fieldValue)
        {
            var append = _fields.Count == 0;
            var fieldNameRoot = fieldName.Substring(0, fieldName.Length - 3);
            var fieldLanguage = fieldName.Substring(fieldName.Length - 2);

            if (_fields.ContainsKey(fieldNameRoot))
            {
                _fields[fieldNameRoot][fieldLanguage] = fieldValue;
            }
            else
            {
                _fields[fieldNameRoot] = new() { [fieldLanguage] = fieldValue };
            }

            return (append, this);
        }

        public override void RenderField(IDictionary<string, object> data)
        {
            foreach (var kv in _fields)
            {
                data[kv.Key] = kv.Value;
            }
        }
    }
}
