namespace Datahub.Portal.Components.TZ;

/// <summary>
/// Represents the context for a local time text template.
/// </summary>
public class LocalDateTimeTemplateContext
{
    /// <summary>
    /// Gets the date and time value in the local time zone.
    /// </summary>
    public DateTimeOffset Value { get; init; }

    /// <summary>
    /// Gets the local time zone information.
    /// </summary>
    public TimeZoneInfo LocalTimeZone { get; init; }

    /// <summary>
    /// Gets the format string for the date and time value.
    /// </summary>
    public string Format { get; init; }

    /// <summary>
    /// Add option to convert the time text to lower case or upper case.
    /// </summary>
    public bool LowerCaseAmPm { get; init; }
}