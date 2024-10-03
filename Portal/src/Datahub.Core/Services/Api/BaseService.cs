using Microsoft.AspNetCore.Components;

namespace Datahub.Core.Services.Api;

public class BaseService
{
    private NavigationManager _navigationManager;

    public BaseService(NavigationManager navigationManager)
    {
        this._navigationManager = navigationManager;
    }
}