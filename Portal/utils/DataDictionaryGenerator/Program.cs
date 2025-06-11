
using Datahub.Core.Model.Context;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

var optionsBuilder = new DbContextOptionsBuilder<SqlServerDatahubContext>();
optionsBuilder.UseSqlServer("Server=(local);Database=dh-portal-projectdb;Trusted_Connection=True;");
// ↑ Replace with your real connection string

using var context = new SqlServerDatahubContext(optionsBuilder.Options);

var model = context.Model;
Console.WriteLine("Generating data dictionary...");
Console.WriteLine($"Found {model.GetEntityTypes().Count()} entities.");
// display current path
Console.WriteLine($"Current directory: {Directory.GetCurrentDirectory()}");
var xmlDocPath = "../../../../../src/Datahub.Core/bin/Debug/net8.0/Datahub.Core.xml"; // Path to your XML doc file
var outputCsvPath = "DataDictionary.csv";

var xmlComments = LoadXmlComments(xmlDocPath);

//using var writer = new StreamWriter(outputCsvPath);
using var writer = Console.Out; // For demonstration, writing to console instead of file
writer.WriteLine("Entity,Property,Type,IsRequired,MaxLength,Summary");

foreach (var entityType in model.GetEntityTypes())
{
    var entityName = entityType.ClrType.Name;
    var entitySummary = xmlComments.GetValueOrDefault($"T:{entityType.ClrType.FullName}");

    foreach (var property in entityType.GetProperties())
    {
        var propName = property.Name;
        var clrType = property.ClrType.Name;
        var isRequired = !property.IsNullable;
        var maxLength = property.GetMaxLength()?.ToString() ?? "";
        var propSummary = xmlComments.GetValueOrDefault($"P:{entityType.ClrType.FullName}.{propName}");

        writer.WriteLine($"\"{entityName}\",\"{propName}\",\"{clrType}\",\"{isRequired}\",\"{maxLength}\",\"{propSummary}\"");
    }
}

Console.WriteLine($"Data dictionary generated: {outputCsvPath}");


static Dictionary<string, string> LoadXmlComments(string xmlPath)
{
    var comments = new Dictionary<string, string>();
    if (!File.Exists(xmlPath)) return comments;

    var doc = XDocument.Load(xmlPath);
    foreach (var member in doc.Descendants("member"))
    {
        var name = member.Attribute("name")?.Value;
        var summary = member.Element("summary")?.Value?.Trim();
        if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(summary))
        {
            comments[name] = summary.Replace("\n", " ").Replace("\r", " ").Trim();
        }
    }
    return comments;
}
