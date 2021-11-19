using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Datahub.CKAN.Package
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
                new CatchAllFieldAgent(string.Empty, string.Empty, true),
                new CatchAllFieldAgent(string.Empty, string.Empty, false)
            };
        }

        public Dictionary<string, object> GeneratePackage(FieldValueContainer fieldValues, string url, bool @private = false)
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

            var requiredFields = fieldValues.Where(f => f.FieldDefinition?.Required_FLAG == true);
            var agents = InstantiateAgents(requiredFields).ToList();
            foreach (var agent in agents)
            {
                agent.RenderField(dict);
            }

            // resources (just the url)
            dict["resources"] = new object[] 
            { 
                new Dictionary<string, object>()
                {
                    { "name_translated", dict["title_translated"] },
                    { "resource_type", "dataset" },
                    { "url", url },
                    { "language", new string[] { "en", "fr" } },
                    { "format", "other" }
                }
            };

            // open government licence - canada
            dict["license_id"] = "ca-ogl-lgo";

            // ready to publish
            dict["ready_to_publish"] = "true";

            return dict;
        }

        private IEnumerable<FieldAgent> InstantiateAgents(IEnumerable<ObjectFieldValue> fieldValues)
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
