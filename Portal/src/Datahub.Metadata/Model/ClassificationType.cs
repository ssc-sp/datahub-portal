using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Text.RegularExpressions;

namespace Datahub.Metadata.Model;

public enum ClassificationType : byte
{ 
    Unclassified,
    ProtectedA,
    ProtectedB
}

public partial class SecurityClassificationStringConverter : ValueConverter<ClassificationType, string>
{
    [GeneratedRegex("protected\\s*a", RegexOptions.IgnoreCase)]
    private static partial Regex ProtectedARegex();

    [GeneratedRegex("protected\\s*b", RegexOptions.IgnoreCase)]
    private static partial Regex ProtectedBRegex();

    [GeneratedRegex("unclassified", RegexOptions.IgnoreCase)]
    private static partial Regex UnclassifiedRegex();

    private static ClassificationType ConvertFromString(string input) => input switch
    {
        string s when ProtectedARegex().IsMatch(s) => ClassificationType.ProtectedA,
        string s when ProtectedBRegex().IsMatch(s) => ClassificationType.ProtectedB,
        string s when UnclassifiedRegex().IsMatch(s) => ClassificationType.Unclassified,
        _ => throw new NotImplementedException()
    };

    public SecurityClassificationStringConverter() : base(v => v.ToString(), v => ConvertFromString(v)) { }
}