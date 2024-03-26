using Datahub.Metadata.DTO;
using Datahub.Metadata.Model;
using Datahub.Metadata.Utils;

namespace Datahub.Infrastructure.Services.Publishing.Package;

public class PackageGenerator
{
    private readonly List<FieldAgent> fieldAgents;

    public PackageGenerator()
    {
        fieldAgents =
        [
            new KeywordFieldAgent(),
            new TranslatedFieldAgent(),
            new CatchAllFieldAgent(string.Empty, string.Empty, true),
            new CatchAllFieldAgent(string.Empty, string.Empty, false)
        ];
    }

    public Dictionary<string, object> GeneratePackage(FieldValueContainer fieldValues, bool allFields, string url = null, bool @private = false)
    {
        if (fieldValues == null)
        {
            throw new ArgumentNullException(nameof(fieldValues));
        }

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

        dict["restrictions"] = "unrestricted";
        dict["owner_org"] = fieldValues[FieldNames.opengov_owner_org]?.Value_TXT;
        dict["date_published"] = fieldValues["date_published"]?.Value_TXT ?? DateTime.UtcNow.ToString("yyyy-MM-dd");

        var requiredFields = fieldValues.Where(f => allFields || f.FieldDefinition?.Required_FLAG == true);
        var agents = InstantiateAgents(requiredFields).ToList();
        foreach (var agent in agents)
        {
            agent.RenderField(dict);
        }

        if (!string.IsNullOrEmpty(url))
        {
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
        }

        // open government licence - canada
        dict["license_id"] = "ca-ogl-lgo";

        // ready to publish
        dict["ready_to_publish"] = "false";
        dict["imso_approval"] = "false";

        return dict;
    }

    private IEnumerable<FieldAgent> InstantiateAgents(IEnumerable<ObjectFieldValue> fieldValues)
    {
        foreach (var fv in fieldValues)
        {
            var definition = fv.FieldDefinition;
            var matchingAgent = fieldAgents.FirstOrDefault(a => a.Matches(definition));
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