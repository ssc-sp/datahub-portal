using NRCan.Datahub.Metadata.DTO;
using NRCan.Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRCan.Datahub.Metadata.CKAN
{
    public class PackageGenerator
    {
        readonly List<FieldAgent> _fieldAgents;

        public PackageGenerator()
        {
            _fieldAgents = new()
            {
                new KeywordFieldAgent(),
                new TranslatedFieldAgent(),
                new CatchAllFieldAgent(true),
                new CatchAllFieldAgent(false)
            };
        }

        public Dictionary<string, object> GeneratePackage(FieldValueContainer fieldValues, bool @private = false)
        {
            Dictionary<string, object> dict = new();

            // package id
            dict["id"] = fieldValues["id"]?.Value_TXT;

            // package name
            dict["name"] = fieldValues["name"]?.Value_TXT;

            // take title english for general title
            dict["title"] = fieldValues["title_translated_en"]?.Value_TXT;

            // private
            dict["private"] = @private;

            // version
            //dict["version"] = null;

            // state is active
            dict["state"] = "active";

            // type is dataset
            dict["type"] = "dataset";

            // no groups
            //dict["groups"] = Array.Empty<object>();

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
}
