using Datahub.Core.Services;
using Microsoft.JSInterop;

namespace Datahub.Portal.Services;

public class DownloadService(IJSRuntime jsRuntime) : IDownloadService
{
    public async Task DownloadStreamAsFile(Stream stream, string filename)
    {
        using var streamReference = new DotNetStreamReference(stream);
        await jsRuntime.InvokeVoidAsync("downloadFileFromStream", filename, streamReference);
    }
}
