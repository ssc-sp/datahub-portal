using System.Text.Json;

namespace Datahub.Core.Model.Datahub
{
    public class CkanPackageBasic
    {
        public const string IMSO_APPROVAL_JSON_PROPERTY_NAME = "imso_approval";
        public const string READY_TO_PUBLISH_JSON_PROPERTY_NAME = "ready_to_publish";
        public const string DATE_PUBLISHED_JSON_PROPERTY_NAME = "date_published";

        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }

        public string DatePublished { get; set; }
        public string Url { get; set; }

        // unfortunately these are stringly-typed booleans (e.g. "imso_approval":"false" instead of "imso_approval":false) and can't be auto-deserialized to bool
        public string ReadyToPublish { get; set; }
        public string ImsoApproval { get; set; }
    }
}
