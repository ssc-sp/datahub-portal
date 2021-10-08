using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.CKAN.Package
{
    public static class PackageGenerator
    {
        static List<FieldAgent> _fieldAgents;

        static PackageGenerator()
        {
            _fieldAgents = new()
            {
                new KeywordFieldAgent(),
                new TranslatedFieldAgent(),
                new CatchAllFieldAgent(string.Empty, string.Empty, true),
                new CatchAllFieldAgent(string.Empty, string.Empty, false)
            };
        }

        public static Dictionary<string, object> GeneratePackage(FieldValueContainer fieldValues, bool @private = false)
        {
            if (fieldValues == null)
                throw new ArgumentNullException(nameof(fieldValues));

            Dictionary<string, object> dict = new();

            // package id
            dict["id"] = fieldValues.ObjectId;

            // package name
            dict["name"] = fieldValues.ObjectId;

            // take title english for general title
            dict["title"] = fieldValues["title_translated_en"]?.Value_TXT ?? string.Empty;

            // private
            dict["private"] = @private;

            // state is active
            dict["state"] = "active";

            // type is dataset
            dict["type"] = "dataset";

            var agents = InstantiateAgents(fieldValues).ToList();
            foreach (var agent in agents)
            {
                agent.RenderField(dict);
            }

            return dict;
        }

        static IEnumerable<FieldAgent> InstantiateAgents(IEnumerable<ObjectFieldValue> fieldValues)
        {
            foreach (var fv in fieldValues)
            {
                var definition = fv.FieldDefinition;
                var matchingAgent = _fieldAgents.FirstOrDefault(a => a.Matches(definition));
                if (matchingAgent != null)
                {
                    var (append, agent) = matchingAgent.Instantiate(definition.Field_Name_TXT, fv.Value_TXT);
                    if (append)
                    {
                        yield return agent;
                    }
                }                
            }
        }
    }
}
