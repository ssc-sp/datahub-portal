using System.Text.Json.Nodes;
using Datahub.Core.Model.Repositories;

namespace Datahub.Core.Data.Databricks;

public class UserPatchRequest
{
    public string[] Schemas { get; set; }
    public UserPatchOperation[] Operations { get; set; }
}
