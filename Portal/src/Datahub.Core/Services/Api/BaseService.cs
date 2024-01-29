using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Services.Api;

public class BaseService
{
    protected NavigationManager _navigationManager;

    public BaseService(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }
}