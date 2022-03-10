using System;
using Microsoft.AspNetCore.Components;
using Datahub.Core.Services;

public class BaseService
{
    protected NavigationManager _navigationManager;
    protected IMyDataService _apiService;
    protected UIControlsService _uiService;

    public BaseService(NavigationManager navigationManager, IMyDataService apiService, UIControlsService uiService)
    {
        _navigationManager = navigationManager;
        _apiService = apiService;
        _uiService = uiService;
    }
}