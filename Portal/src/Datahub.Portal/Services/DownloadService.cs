using Datahub.Core.Services;
using Microsoft.JSInterop;

namespace Datahub.Portal.Services;

public class DownloadService(IJSRuntime jsRuntime) : IDownloadService
{
    private IJSObjectReference _downloadStreamModule = default;

    public async Task DownloadStreamAsFile(Stream stream, string filename)
    {
        var module = await GetDownloadStreamModule();

        using var streamReference = new DotNetStreamReference(stream);
        await module.InvokeVoidAsync("downloadFileFromStream", filename, streamReference);
    }

    private async Task<IJSObjectReference> GetDownloadStreamModule()
    {
        if (_downloadStreamModule == default)
        {
            _downloadStreamModule = await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/DownloadFileFromStream.js");
        }

        return _downloadStreamModule;
    }
}
