using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Datahub.Infrastructure.Services.Storage;

public static class AzureStorageUtils
{
    public static string BuildAzureStorageConnectionString(string accountName, string accountKey)
    {
        return $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};EndpointSuffix=core.windows.net";
    }
}
