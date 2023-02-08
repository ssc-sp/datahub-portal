using System.Text.Json.Nodes;

namespace Datahub.Shared.Entities;

public class TerraformWorkspace
{
    public string? Name { get; set; }
    public string? Acronym { get; set; }
    public TerraformOrganization? TerraformOrganization { get; set; }
    public IEnumerable<TerraformUser>? Users { get; set; }

    public JsonNode ToUserList()
    {
        var users = new JsonArray();

        if (Users == null)
        {
            return users;
        }
        
        foreach (var user in Users)
        {
            var userJson = new JsonObject()
            {
                ["email"] = user.Email,
                ["oid"] = user.ObjectId,
            };
            users.Add(userJson);
        }

        return users;
    }
}