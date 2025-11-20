using Newtonsoft.Json;

namespace Datahub.Core.Data.Databricks
{
    /// <summary>
    /// Represents a Databricks user following the SCIM 2.0 User Schema.
    /// API Reference: https://docs.databricks.com/api/azure/workspace/usersv2/create
    /// SCIM 2.0 Spec: https://datatracker.ietf.org/doc/html/rfc7643#section-4.1
    /// </summary>
    public class DatabricksUser
    {
        /// <summary>Gets or sets the SCIM schemas. Required field.</summary>
        public List<string> schemas { get; set; } = new();

        /// <summary>Gets or sets the Databricks user ID. Response-only field - do not include when creating.</summary>
        public string? id { get; set; }

        /// <summary>Gets or sets the user's email address. Required for create operations.</summary>
        public string userName { get; set; } = null!;

        /// <summary>Gets or sets the email addresses associated with the user. Optional.</summary>
        public List<Email>? emails { get; set; }

        /// <summary>Gets or sets the user's name (givenName and familyName). Optional.</summary>
        public Name? name { get; set; }

        /// <summary>Gets or sets the user's display name. Optional.</summary>
        public string? displayName { get; set; }

        /// <summary>Gets or sets the groups the user belongs to. Optional.</summary>
        public List<Group>? groups { get; set; }

        /// <summary>Gets or sets the roles associated with the user. Optional.</summary>
        public List<Role>? roles { get; set; }

        /// <summary>Gets or sets the entitlements associated with the user. Optional.</summary>
        public List<Entitlement>? entitlements { get; set; }

        /// <summary>Gets or sets the external ID (e.g., Azure AD Object ID). Optional.</summary>
        public string? externalId { get; set; }

        /// <summary>Gets or sets a value indicating whether the user is active. Default: true.</summary>
        public bool active { get; set; } = true;

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
            emails = value.emails;
            name = value.name;
            displayName = value.displayName;
            groups = value.groups;
            roles = value.roles;
            entitlements = value.entitlements;
            externalId = value.externalId;
            active = value.active;
        }
    }

    /// <summary>
    /// Email address in SCIM 2.0 format.
    /// Reference: https://datatracker.ietf.org/doc/html/rfc7643#section-2.4
    /// </summary>
    public class Email
    {
        [JsonProperty("$ref")]
        public string? @ref { get; set; }
        public string value { get; set; } = null!;
        public string? display { get; set; }
        public bool primary { get; set; } = false;
        public string? type { get; set; }
    }

    /// <summary>Entitlement in SCIM 2.0 format.</summary>
    public class Entitlement
    {
        [JsonProperty("$ref")]
        public string? @ref { get; set; }
        public string value { get; set; } = null!;
        public string? display { get; set; }
        public bool primary { get; set; } = false;
        public string? type { get; set; }
    }

    /// <summary>
    /// Group membership in SCIM 2.0 format.
    /// Groups API: https://docs.databricks.com/api/azure/workspace/groups
    /// </summary>
    public class Group
    {
        [JsonProperty("$ref")]
        public string? @ref { get; set; }
        public string value { get; set; } = null!;
        public string? display { get; set; }
        public bool primary { get; set; } = false;
        public string? type { get; set; }
    }

    /// <summary>User's name in SCIM 2.0 format.</summary>
    public class Name
    {
        public string? givenName { get; set; }
        public string? familyName { get; set; }
    }

    /// <summary>Role assignment in SCIM 2.0 format.</summary>
    public class Role
    {
        [JsonProperty("$ref")]
        public string? @ref { get; set; }
        public string value { get; set; } = null!;
        public string? display { get; set; }
        public bool primary { get; set; } = false;
        public string? type { get; set; }
    }

    /// <summary>
    /// List of Databricks users from list/search operations.
    /// List API: https://docs.databricks.com/api/azure/workspace/usersv2/list
    /// SCIM Spec: https://datatracker.ietf.org/doc/html/rfc7644#section-3.4.2
    /// </summary>
    public class DatabricksUserList
    {
        public List<string> schemas { get; set; } = new();
        public int totalResults { get; set; } = 0;
        public int startIndex { get; set; } = 0;
        public int itemsPerPage { get; set; } = 0;
        public List<DatabricksUser> Resources { get; set; } = new();
    }
}
