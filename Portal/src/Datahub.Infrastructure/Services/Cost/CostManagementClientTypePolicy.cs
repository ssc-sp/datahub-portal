using Azure.Core;
using Azure.Core.Pipeline;

namespace Datahub.Infrastructure.Services.Cost;

internal sealed class CostManagementClientTypePolicy(string clientType) : HttpPipelineSynchronousPolicy
{
    private const string CostManagementPath = "/providers/Microsoft.CostManagement/";

    public override void OnSendingRequest(HttpMessage message)
    {
        if (AppliesTo(message.Request.Uri.Path))
        {
            message.Request.Headers.SetValue("ClientType", clientType);
        }
    }

    internal static bool AppliesTo(string path)
    {
        return path.Contains(CostManagementPath, StringComparison.OrdinalIgnoreCase);
    }
}
