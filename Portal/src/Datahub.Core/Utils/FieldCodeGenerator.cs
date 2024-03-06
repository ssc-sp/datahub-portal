using System.Text;
using System.Text.RegularExpressions;
using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Utils;

public class FieldCodeGenerator
{
    private readonly Func<string, int> sectionMapper;

    public const char ChoiceSeparator = '|';

    public FieldCodeGenerator(Func<string, int> sectionMapper)
    {
        this.sectionMapper = sectionMapper;
    }

    public string GetFormattedFieldName(WebFormField field)
    {
        if (string.IsNullOrEmpty(field.FieldDESC))
            return string.Empty;

        var deDashed = field.FieldDESC.Replace("-", string.Empty);
        return Regex.Replace(deDashed, "[^A-Za-z0-9_]+", "_", RegexOptions.Compiled);
    }

    public string GenerateSQLName(WebFormField field)
    {
        var formatted = GetFormattedFieldName(field);
        return "NONE".Equals(field.ExtensionCD) ? formatted : $"{formatted}_{field.ExtensionCD}";
    }

    public string GenerateJSON(WebFormField field)
    {
        return $"{Quote(GenerateSQLName(field))}: {Quote(System.Web.HttpUtility.JavaScriptStringEncode(field.FieldDESC))}";
    }

    public string GenerateCSharp(WebFormField field)
    {
        StringBuilder sb = new();

        // EFCoreAnnotation2: required
        if (field.MandatoryFLAG)
        {
            sb.AppendLine("[Required]");
        }

        // EFCoreAnnotation3: MaxLength
        if (field.MaxLengthNUM.HasValue)
        {
            sb.AppendLine($"[MaxLength({field.MaxLengthNUM.Value})]");
        }

        // AeFormCategory
        if (!string.IsNullOrEmpty(field.SectionDESC))
        {
            var sectionIndex = sectionMapper?.Invoke(field.SectionDESC) ?? 1;
            sb.AppendLine($"[AeFormCategory({Quote(field.SectionDESC)}, {sectionIndex})]");
        }

        // AeLabel
        TryRenderLabel(sb, field);

        // EFCoreAnnotation1: field type
        if (FormFieldTypeReference.Annotations.ContainsKey(field.TypeCD))
        {
            sb.AppendLine(FormFieldTypeReference.Annotations[field.TypeCD]);
        }

        var fieldType = FormFieldTypeReference.EFTypes[field.TypeCD];
        sb.AppendLine($"public {fieldType} {GenerateSQLName(field)} {{ get; set; }}");

        return sb.ToString();
    }

    internal static void TryRenderLabel(StringBuilder sb, WebFormField field)
    {
        var isDropdown = "Dropdown".Equals(field.TypeCD);
        var hasPlaceHolder = !string.IsNullOrEmpty(field.DescriptionDESC);
        if (isDropdown || hasPlaceHolder)
        {
            sb.Append("[AeLabel(");

            if (isDropdown)
                sb.Append("isDropDown: true");

            if (hasPlaceHolder)
            {
                if (isDropdown)
                    sb.Append(", ");

                var placeholder = Quote($"[{field.DescriptionDESC}]");
                sb.Append($"placeholder: {placeholder}");
            }

            var validValues = GetValidValues(field.ChoicesTXT);
            if (!string.IsNullOrEmpty(validValues))
            {
                sb.Append(", ").Append(validValues);
            }

            sb.Append(")]\n");
        }
    }

    internal static string GetValidValues(string choices)
    {
        if (string.IsNullOrEmpty(choices))
            return string.Empty;

        var splitChoices = choices
            .Split(ChoiceSeparator, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c));

        var validValues = string.Join(", ", splitChoices.Select(Quote));

        return $"validValues: new [] {{ {validValues} }}";
    }

    internal static string Quote(string value) => $"\"{value.Replace("\"", "\"")}\"";
}