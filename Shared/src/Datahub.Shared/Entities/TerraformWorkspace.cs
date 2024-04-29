#nullable enable

// This is actually required by some projects
// ReSharper disable once RedundantUsingDirective
// This is needed, even if your IDE says it isn't
using System.Text.Json.Nodes;
using Datahub.Shared.Enums;

namespace Datahub.Shared.Entities;

public class TerraformWorkspace
{
    public const string DefaultVersion = "latest";
    public string? Name { get; set; }
    public string? Acronym { get; set; }
    public double BudgetAmount { get; set; } = 100;

    public string Version { get; set; } = DefaultVersion;

    // TODO: Move this into the storage module
    public int StorageSizeLimitInTB { get; set; } = 5;
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

    public JsonNode ToUserList(Role? role)
    {
        var users = new JsonArray();

        if (Users == null)
        {
            return users;
        }

        foreach (var user in Users)
        {
            if (role.HasValue && user.Role != role)
                continue;

            var userJson = new JsonObject()
            {
                ["email"] = user.Email,
                ["oid"] = user.ObjectId,
            };
            users.Add(userJson);
        }

        return users;
    }

    public JsonNode ToUserList(List<Role> roles)
    {
        var users = new JsonArray();

        if (Users == null)
        {
            return users;
        }

        foreach (var user in Users)
        {
            if (roles.Any(r => r == user.Role))
            {
                var userJson = new JsonObject()
                {
                    ["email"] = user.Email,
                    ["oid"] = user.ObjectId,
                };
                users.Add(userJson);
            }
        }

        return users;
    }
}