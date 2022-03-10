using System;
using Microsoft.AspNetCore.Components;
using Datahub.Core.Services;

public class BaseService
{
    protected NavigationManager _navigationManager;
    protected ApiService _apiService;
    protected UIControlsService _uiService;

    public BaseService(NavigationManager navigationManager, ApiService apiService, UIControlsService uiService)
    {
        _navigationManager = navigationManager;
        _apiService = apiService;
        _uiService = uiService;
    }
}