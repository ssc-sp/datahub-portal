using Datahub.Metadata.Model;

namespace Datahub.Core;

public static class SecurityClassification
{
    public const string Unclassified = "Unclassified";
    public const string ProtectedA = "Protected A";
    public const string ProtectedB = "Protected B";

    public static string GetSecurityClassification(ClassificationType classificationType)
    {
        return classificationType switch
        {
            ClassificationType.ProtectedA => ProtectedA,
            ClassificationType.ProtectedB => ProtectedB,
            _ => Unclassified
        };
    }
}