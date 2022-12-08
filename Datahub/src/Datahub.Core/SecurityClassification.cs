using Datahub.Metadata.Model;

public static class SecurityClassification
{
    public const string Unclassified = "Unclassified";
    public const string ProtectedA = "Protected A";
    public const string ProtectedB = "Protected B";

    public static string GetSecurityClassification(ClassificationType classificationType)
    {
        return classificationType switch
        {
            ClassificationType.ProtectedA => SecurityClassification.ProtectedA,
            ClassificationType.ProtectedB => SecurityClassification.ProtectedB,
            _ => SecurityClassification.Unclassified
        };
    }
}
