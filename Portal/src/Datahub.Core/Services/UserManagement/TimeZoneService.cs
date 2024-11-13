using Microsoft.JSInterop;

namespace Datahub.Core.Services.UserManagement;

/// <summary>
/// https://www.meziantou.net/convert-datetime-to-user-s-time-zone-with-server-side-blazor.htm
/// </summary>
public sealed class TimeZoneService
{
    private readonly IJSRuntime _jsRuntime;

    private TimeSpan? _userOffset;

    public TimeZoneService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<DateTimeOffset> LocalDateTime(DateTimeOffset dateTime)
    {
        if (_userOffset == null)
        {
            int offsetInMinutes = await _jsRuntime.InvokeAsync<int>("blazorGetTimezoneOffset");
            _userOffset = TimeSpan.FromMinutes(-offsetInMinutes);
        }
        return dateTime.ToOffset(_userOffset.Value);
    }
}