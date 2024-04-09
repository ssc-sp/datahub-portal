using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Datahub.Core.Services;

public class CustomNavigation
{
    private readonly NavigationManager navigationManager;
    private readonly IJSRuntime jsInterop;

    public CustomNavigation(NavigationManager navigationManager, IJSRuntime jsInterop)
    {
        this.navigationManager = navigationManager;
        this.jsInterop = jsInterop;
    }

    public string Uri => navigationManager.Uri;

    public async Task NavigateTo(string url, bool overrideHistory)
    {
        if (overrideHistory)
        {
            await jsInterop.InvokeVoidAsync("overrideHistory", url);
        }
        else
        {
            navigationManager.NavigateTo(url);
        }
    }

    public Uri GetAbsoluteUri() => navigationManager.ToAbsoluteUri(navigationManager.Uri);
}