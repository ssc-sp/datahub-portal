using System.Text.Json.Nodes;

namespace ResourceProvisioner.Domain.Entities;

public class TerraformWorkspace
{
    public string Name { get; set; }
    public string Acronym { get; set; }
    public Organization Organization { get; set; }
    public List<TerraformWorkspaceUser> Users { get; set; }

    public JsonNode ToUserList()
    {
        var users = new JsonArray();
        foreach (var user in Users)
        {
            var userJson = new JsonObject();
            userJson.Add("email", user.Email);
            users.Add(userJson);
        }

        return users;
    }
}