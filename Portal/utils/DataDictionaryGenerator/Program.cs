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

using var writer = new StreamWriter(outputCsvPath);
//using var writer = Console.Out; // For demonstration, writing to console instead of file
writer.WriteLine("Entity,Property,Type,IsRequired,MaxLength,Summary");

foreach (var entityType in model.GetEntityTypes())
{
    var entityName = entityType.ClrType.Name;
    var entitySummary = xmlComments.GetValueOrDefault($"T:{entityType.ClrType.FullName}");

    // Add a line for the entity itself only if it's not an abstract class
    if (!entityType.ClrType.IsAbstract && !string.IsNullOrEmpty(entitySummary))
    {
        writer.WriteLine($"\"{entityName}\",\"(Class Level)\",\"\",\"\",\"\",\"{entitySummary}\"");
    }

    // If the entity type is abstract, we still want its properties to be documented
    // under its concrete derived classes. EF Core's GetProperties() on a derived type
    // will include properties from its abstract base types if they are part of the model.
    // So, we only skip the "(Class Level)" line for abstract types, not processing their properties here.
    // However, if the request implies that abstract classes should not be processed AT ALL,
    // meaning their properties are also not listed even under derived classes through this loop,
    // then a `if (entityType.ClrType.IsAbstract) continue;` would be needed here.
    // Based on "include in child classes the documentation from parent properties",
    // we should process all entityTypes to correctly attribute properties to their concrete classes.

    // Process properties for all entity types (abstract or concrete)
    // because concrete types will inherit these properties.
    // The entityName in the CSV will be that of the concrete class.
    if (entityType.ClrType.IsAbstract)
    {
        // If it's an abstract class, we don't list its properties directly under its own name.
        // They will be listed under the concrete derived classes.
        // So, if we are iterating an abstract class here, we can skip its properties loop,
        // as they will be covered when we iterate over its concrete children.
        // However, GetEntityTypes() returns all types in the model. If an abstract type
        // has no concrete derived types *also in the model*, its properties might be missed.
        // It's safer to iterate all properties of all entity types, and the `entityName`
        // correctly reflects the current `entityType` being processed.
        // The key is that `GetProperties()` on a *concrete* type will include inherited ones.

        // If the goal is to only output rows for concrete classes, then:
        if (entityType.ClrType.IsAbstract) continue; // Skip abstract classes entirely
    }

    foreach (var property in entityType.GetProperties()) // Gets all properties, including inherited ones for the current entityType
    {
        var propName = property.Name;
        var clrType = property.ClrType.Name;
        var isRequired = !property.IsNullable;
        var maxLength = property.GetMaxLength()?.ToString() ?? "";

        // Determine the correct full name of the type that declared the property for XML comment lookup
        // For shadow properties, property.PropertyInfo will be null.
        // For regular properties, property.PropertyInfo.DeclaringType will give the C# type that declared it.
        var xmlDocKeyOwnerFullName = property.PropertyInfo?.DeclaringType?.FullName ?? entityType.ClrType.FullName;
        var propSummary = xmlComments.GetValueOrDefault($"P:{xmlDocKeyOwnerFullName}.{propName}");

        if (!string.IsNullOrEmpty(propSummary))
        {
            if (propSummary.StartsWith("Gets or sets "))
            {
                propSummary = propSummary.Substring("Gets or sets ".Length);
            }
            else if (propSummary.StartsWith("Gets a value indicating "))
            {
                propSummary = propSummary.Substring("Gets a value indicating ".Length);
            }

            if (!string.IsNullOrEmpty(propSummary)) // Check again in case the substring operations made it empty
            {
                propSummary = char.ToUpper(propSummary[0]) + propSummary.Substring(1);
            }
        }

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
