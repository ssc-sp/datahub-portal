using System.Runtime.InteropServices;

namespace Datahub.LanguageTraining.Utils;

public static class TimeZoneUtils
{
	public static DateTime ConvertUtcToEasternTime(this DateTime value)
	{
		TimeZoneInfo tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
			? TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time")
			: TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
		return TimeZoneInfo.ConvertTimeFromUtc(value, tz);
	}
}