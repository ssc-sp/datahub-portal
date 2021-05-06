using System;
using Microsoft.AspNetCore.Components;
using NRCan.Datahub.Shared.Services;

public class BaseService
{
    protected NavigationManager _navigationManager;
    protected IApiService _apiService;
    protected UIControlsService _uiService;

    public BaseService(NavigationManager navigationManager, IApiService apiService, UIControlsService uiService)
    {
        _navigationManager = navigationManager;
        _apiService = apiService;
        _uiService = uiService;
    }

    public void DisplayErrorUI(Exception ex)
    {
        this._apiService.LastException = ex;
        this._uiService.ShowErrorModal();
    }
}