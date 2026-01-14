#nullable enable
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Configuration;

/// <summary>
/// Utility to safely dump objects (like configuration) by masking sensitive values.
/// Intended for diagnostics in local and Functions environments.
/// </summary>
public static class ConfigurationHelper
{
    private static readonly JsonSerializerOptions PrettyJson = new(JsonSerializerDefaults.General)
    {
        WriteIndented = true
    };

    public static string GetCurrentEnvironment(this IConfiguration configuration) => configuration["DataHub_ENVNAME"] ?? "dev";

    // Property name tokens considered sensitive (case-insensitive).
    private static readonly string[] SensitivePropertyTokens =
    [
        "secret", "password", "token", "key", "connectionstring", "clientsecret", "thumbprint"
    ];

    /// <summary>
    /// Produces a JSON string representation of the provided value with sensitive properties redacted.
    /// </summary>
    /// <param name="value">The object to serialize and redact.</param>
    /// <returns>A pretty-printed JSON string with sensitive values masked.</returns>
    public static string ToRedactedJson(object? value)
        => JsonSerializer.Serialize(RedactObject(value), PrettyJson);

    /// <summary>
    /// Produces a JSON string representation of a configuration section with sensitive values redacted.
    /// </summary>
    /// <param name="section">The configuration section to serialize and redact.</param>
    /// <returns>A pretty-printed JSON string with sensitive values masked.</returns>
    public static string ToRedactedJson(IConfigurationSection section)
        => JsonSerializer.Serialize(SectionToObject(section), PrettyJson);

    /// <summary>
    /// Writes a redacted JSON dump to the console, wrapped with a header.
    /// </summary>
    /// <param name="title">A short title describing the dump.</param>
    /// <param name="value">The object to serialize and redact.</param>
    public static void DumpRedactedToConsole(string title, object? value)
    {
        try
        {
            Console.WriteLine($"===== {title} (redacted) =====");
            Console.WriteLine(ToRedactedJson(value));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to dump '{title}': {ex}");
        }
    }

    /// <summary>
    /// Writes a redacted JSON dump of a configuration section to the console, wrapped with a header.
    /// </summary>
    /// <param name="title">A short title describing the dump.</param>
    /// <param name="section">The configuration section to serialize and redact.</param>
    public static void DumpRedactedToConsole(string title, IConfigurationSection section)
    {
        try
        {
            Console.WriteLine($"===== {title} (redacted) =====");
            Console.WriteLine(ToRedactedJson(section));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to dump '{title}': {ex}");
        }
    }

    private static bool IsSensitive(string propertyName)
        => SensitivePropertyTokens.Any(t => propertyName.Contains(t, StringComparison.OrdinalIgnoreCase));

    private static object? RedactObject(object? value, int depth = 0, HashSet<object>? visited = null)
    {
        const int maxDepth = 8;
        if (value is null) return null;
        if (depth > maxDepth) return "[max-depth]";

        var type = value.GetType();

        // Primitives and commonly serializable leaf types
        if (type.IsPrimitive || value is string || value is decimal || value is DateTime || value is DateTimeOffset || value is Guid || value is TimeSpan || value is Enum)
            return value;

        visited ??= new HashSet<object>(ReferenceEqualityComparer.Instance);
        if (!visited.Add(value)) return "[circular]";

        // IEnumerable (non-dictionary)
        if (value is IEnumerable enumerable and not IDictionary)
        {
            var list = new List<object?>();
            foreach (var item in enumerable)
                list.Add(RedactObject(item, depth + 1, visited));
            return list;
        }

        // IDictionary
        if (value is IDictionary dict)
        {
            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in dict)
            {
                var key = entry.Key?.ToString() ?? "(null)";
                result[key] = IsSensitive(key) ? Mask(entry.Value) : RedactObject(entry.Value, depth + 1, visited);
            }
            return result;
        }

        // Complex object: project public readable properties
        var obj = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetIndexParameters().Length > 0) continue;
            if (prop.GetMethod is null) continue;

            object? propValue;
            try { propValue = prop.GetValue(value); }
            catch { propValue = "[unreadable]"; }

            obj[prop.Name] = IsSensitive(prop.Name) ? Mask(propValue) : RedactObject(propValue, depth + 1, visited);
        }
        return obj;
    }

    private static object? SectionToObject(IConfigurationSection section, int depth = 0)
    {
        const int maxDepth = 16;
        if (depth > maxDepth) return "[max-depth]";

        // Children first – if no children, treat as leaf
        var children = section.GetChildren();
        using var enumerator = children.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            // Leaf value – redact based on key name
            return IsSensitive(section.Key) ? Mask(section.Value) : section.Value;
        }

        // Has children – build object
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var child in children)
        {
            var childObj = SectionToObject(child, depth + 1);
            if (IsSensitive(child.Key) && childObj is string s)
            {
                dict[child.Key] = Mask(s);
            }
            else
            {
                dict[child.Key] = childObj;
            }
        }
        return dict;
    }

    private static string Mask(object? original)
    {
        if (original is null) return "(null)";
        var s = original.ToString() ?? string.Empty;
        if (s.Length <= 4) return "****";
        return $"{new string('*', Math.Min(8, s.Length - 4))}{s[^4..]}";
    }

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }
}
