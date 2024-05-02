using Newtonsoft.Json;

namespace Datahub.Core.Data.Databricks
{
    public class DatabricksUser
    {
        public List<string> schemas { get; set; }
        public string id { get; set; }
        public string userName { get; set; }
        public List<Email> emails { get; set; }
        public Name name { get; set; }
        public string displayName { get; set; }
        public List<Group> groups { get; set; }
        public List<Role> roles { get; set; }
        public List<Entitlement> entitlements { get; set; }
        public string externalId { get; set; }
        public bool active { get; set; }

        public DatabricksUser()
        {
            schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:User" };
        }

        public DatabricksUser(string json)
        {
            var value = JsonConvert.DeserializeObject<DatabricksUser>(json);
            schemas = value.schemas;
            id = value.id;
            userName = value.userName;
            displayName = value.displayName;
            groups = value.groups;
            roles = value.roles;
            entitlements = value.entitlements;
            externalId = value.externalId;
            active = value.active;
        }
    }

    public class Email
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; }
        public string value { get; set; }
        public string display { get; set; }
        public bool primary { get; set; }
        public string type { get; set; }
    }

    public class Entitlement
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; }
        public string value { get; set; }
        public string display { get; set; }
        public bool primary { get; set; }
        public string type { get; set; }
    }

    public class Group
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; }
        public string value { get; set; }
        public string display { get; set; }
        public bool primary { get; set; }
        public string type { get; set; }
    }

    public class Name
    {
        public string givenName { get; set; }
        public string familyName { get; set; }
    }

    public class Role
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; }
        public string value { get; set; }
        public string display { get; set; }
        public bool primary { get; set; }
        public string type { get; set; }
    }

    public class DatabricksUserList
    {
        public List<string> schemas { get; set; }
        public int totalResults { get; set; }
        public int startIndex { get; set; }
        public int itemsPerPage { get; set; }
        public List<DatabricksUser> Resources { get; set; }
    }
}
