using Newtonsoft.Json;

namespace Datahub.Core.Data.Databricks
{
    public class DatabricksUser
    {
        public List<string> schemas { get; set; } = new();
        public string id { get; set; } = null!;
        public string userName { get; set; } = null!;
        public List<Email> emails { get; set; } = new();
        public Name name { get; set; } = null!;
        public string displayName { get; set; } = null!;
        public List<Group> groups { get; set; } = new();
        public List<Role> roles { get; set; } = new();
        public List<Entitlement> entitlements { get; set; } = new();
        public string externalId { get; set; } = null!;
        public bool active { get; set; } = false;

        public DatabricksUser()
        {
            schemas = new List<string> { "urn:ietf:params:scim:schemas:core:2.0:User" };
        }

        public DatabricksUser(string json)
        {
            var value = JsonConvert.DeserializeObject<DatabricksUser>(json);
            if (value is null) throw new InvalidOperationException("Deserialization resulted in null DatabricksUser");
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
        public string @ref { get; set; } = null!;
        public string value { get; set; } = null!;
        public string display { get; set; } = null!;
        public bool primary { get; set; } = false;
        public string type { get; set; } = null!;
    }

    public class Entitlement
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; } = null!;
        public string value { get; set; } = null!;
        public string display { get; set; } = null!;
        public bool primary { get; set; } = false;
        public string type { get; set; } = null!;
    }

    public class Group
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; } = null!;
        public string value { get; set; } = null!;
        public string display { get; set; } = null!;
        public bool primary { get; set; } = false;
        public string type { get; set; } = null!;
    }

    public class Name
    {
        public string givenName { get; set; } = null!;
        public string familyName { get; set; } = null!;
    }

    public class Role
    {
        [JsonProperty("$ref")]
        public string @ref { get; set; } = null!;
        public string value { get; set; } = null!;
        public string display { get; set; } = null!;
        public bool primary { get; set; } = false;
        public string type { get; set; } = null!;
    }

    public class DatabricksUserList
    {
        public List<string> schemas { get; set; } = new();
        public int totalResults { get; set; } = 0;
        public int startIndex { get; set; } = 0;
        public int itemsPerPage { get; set; } = 0;
        public List<DatabricksUser> Resources { get; set; } = new();
    }
}
