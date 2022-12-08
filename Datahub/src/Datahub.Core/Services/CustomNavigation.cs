﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Datahub.Core.Services
{
    public class CustomNavigation
    {
        private readonly NavigationManager _navigationManager;
        private readonly IJSRuntime _jsInterop;

        public CustomNavigation(NavigationManager navigationManager, IJSRuntime jsInterop)
        {
            _navigationManager = navigationManager;
            _jsInterop = jsInterop;
        }

        public string Uri => _navigationManager.Uri;

        public async Task NavigateTo(string url, bool overrideHistory)
        {
            if (overrideHistory)
            {
                await _jsInterop.InvokeVoidAsync("overrideHistory", url);
            }
            else
            {
                _navigationManager.NavigateTo(url);
            }
        }

        public Uri GetAbsoluteUri() => _navigationManager.ToAbsoluteUri(_navigationManager.Uri);
    }
}
