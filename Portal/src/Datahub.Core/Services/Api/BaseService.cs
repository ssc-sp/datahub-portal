using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Services.Api;

public class BaseService
{
    protected NavigationManager NavigationManager { get; private set; }

    public BaseService(NavigationManager navigationManager)
    {
        this.NavigationManager = navigationManager;
    }
}