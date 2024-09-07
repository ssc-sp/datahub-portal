using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System;
using Microsoft.AspNetCore.Components.Routing;
using System.Collections.Generic;
using Datahub.Application.Services.Achievements;

namespace Datahub.Tests;

public interface INavigationManager
{
    void NavigateTo(string uri, NavigationOptions options);
    string LastUri { get; }
}

public class FakeNavigationManager : NavigationManager, INavigationManager
{ 
    private static string _uri;
    public NavigationOptions Options { get; private set; }
    public string LastUri { get { return _uri; } }

    public event EventHandler<LocationChangedEventArgs> LocationChanged;

    public FakeNavigationManager()
    {
        Initialize("https://example.com/", "https://example.com/");
    }

    protected override void NavigateToCore(string uri, NavigationOptions options)
    {
        Options = options;
        _uri = uri;
    }
    public void SimulateLocationChange(string newUri)
    {
        bool IsNavigationIntercepted = false;
        var args = new LocationChangedEventArgs(newUri, IsNavigationIntercepted);
        LocationChanged?.Invoke(this, args);
    }
}
public class FakePortalUserTelemetryService : IPortalUserTelemetryService
{
    public event EventHandler<AchievementsEarnedEventArgs> OnAchievementsEarned;

    public FakePortalUserTelemetryService()
    {
    }

    public void SimulatechievementsEarned(List<string> events)
    { 
        var args = new AchievementsEarnedEventArgs(events, false);
        OnAchievementsEarned?.Invoke(this, args);
    }

    public Task LogTelemetryEvent(string eventName)
    {
        throw new NotImplementedException();
    }
}
