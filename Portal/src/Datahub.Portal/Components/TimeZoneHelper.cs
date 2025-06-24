using Microsoft.JSInterop;

namespace Datahub.Portal.Components;

public class TimeZoneHelper
{
    private readonly IJSRuntime _jsRuntime;

    public TimeZoneHelper(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<DateTime> ToUserLocalTimeAsync(DateTime utcDateTime)
    {
        // Get the user's timezone offset in minutes (difference from UTC)
        int offsetMinutes = await _jsRuntime.InvokeAsync<int>("blazorGetTimezoneOffset");
        // Subtract the offset to get local time (offset is negative for UTC+)
        return utcDateTime.AddMinutes(-offsetMinutes);
    }
}
