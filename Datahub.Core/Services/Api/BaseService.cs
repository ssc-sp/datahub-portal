using System;
using Microsoft.AspNetCore.Components;
using Datahub.Core.Services;

public class BaseService
{
    protected NavigationManager _navigationManager;
    protected UIControlsService _uiService;

    public BaseService(NavigationManager navigationManager, UIControlsService uiService)
    {
        _navigationManager = navigationManager;
        _uiService = uiService;
    }
}