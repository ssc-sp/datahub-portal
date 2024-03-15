using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Services.Api;

public class BaseService
{
    protected NavigationManager navigationManager;

    public BaseService(NavigationManager navigationManager)
    {
        this.navigationManager = navigationManager;
    }
}