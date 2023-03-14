namespace Datahub.Functions;

internal static class TemplateUtils
{
    public static (string? Subject, string? Body) GetEmailTemplate(string fileName)
    {
        var filePath = $"./templates/{fileName}";
        if (!File.Exists(filePath))
            return (null, null);

        var lines = File.ReadAllLines(filePath);
        if (lines is null) 
            return (null, null);

        var subject = lines.FirstOrDefault();
        var body = string.Join("\n", lines.Skip(1));

        return (subject, body);
    }
}
