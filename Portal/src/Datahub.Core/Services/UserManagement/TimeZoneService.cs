using Microsoft.JSInterop;

namespace Datahub.Core.Services.UserManagement;

/// <summary>
/// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor.htm
/// </summary>
public sealed class TimeZoneService
{
    private readonly IJSRuntime jsRuntime;

    private TimeSpan? userOffset;

    public TimeZoneService(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime;
    }

    public async ValueTask<DateTimeOffset> GetLocalDateTime(DateTimeOffset dateTime)
    {
        if (userOffset == null)
        {
            int offsetInMinutes = await jsRuntime.InvokeAsync<int>("blazorGetTimezoneOffset");
            userOffset = TimeSpan.FromMinutes(-offsetInMinutes);
        }
        return dateTime.ToOffset(userOffset.Value);
    }
}