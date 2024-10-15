using System.Text;
using System.Text.RegularExpressions;
using Datahub.Core.Model.Datahub;

namespace Datahub.Core.Utils;

public class FieldCodeGenerator
{
    private const char ChoiceSeparator = '|';

    private readonly Func<string, int> _sectionMapper;

    public FieldCodeGenerator(Func<string, int> sectionMapper)
    {
        this._sectionMapper = sectionMapper;
    }

    public string GetFormattedFieldName(WebForm_Field field)
    {
        if (string.IsNullOrEmpty(field.Field_DESC))
            return string.Empty;

        var deDashed = field.Field_DESC.Replace("-", string.Empty);
        return Regex.Replace(deDashed, "[^A-Za-z0-9_]+", "_", RegexOptions.Compiled);
    }

    public string GenerateSQLName(WebForm_Field field)
    {
        var formatted = GetFormattedFieldName(field);
        return "NONE".Equals(field.Extension_CD) ? formatted : $"{formatted}_{field.Extension_CD}";
    }

    public string GenerateJSON(WebForm_Field field)
    {
        return $"{Quote(GenerateSQLName(field))}: {Quote(System.Web.HttpUtility.JavaScriptStringEncode(field.Field_DESC))}";
    }

    public string GenerateCSharp(WebForm_Field field)
    {
        StringBuilder sb = new();

        // EFCoreAnnotation2: required
        if (field.Mandatory_FLAG)
        {
            sb.AppendLine("[Required]");
        }

        // EFCoreAnnotation3: MaxLength
        if (field.Max_Length_NUM.HasValue)
        {
            sb.AppendLine($"[MaxLength({field.Max_Length_NUM.Value})]");
        }

        // AeFormCategory
        if (!string.IsNullOrEmpty(field.Section_DESC))
        {
            var sectionIndex = _sectionMapper?.Invoke(field.Section_DESC) ?? 1;
            sb.AppendLine($"[AeFormCategory({Quote(field.Section_DESC)}, {sectionIndex})]");
        }

        // AeLabel
        TryRenderLabel(sb, field);

        // EFCoreAnnotation1: field type
        if (FormFieldTypeReference.Annotations.ContainsKey(field.Type_CD))
        {
            sb.AppendLine(FormFieldTypeReference.Annotations[field.Type_CD]);
        }

        var fieldType = FormFieldTypeReference.EFTypes[field.Type_CD];
        sb.AppendLine($"public {fieldType} {GenerateSQLName(field)} {{ get; set; }}");

        return sb.ToString();
    }

    private static void TryRenderLabel(StringBuilder sb, WebForm_Field field)
    {
        var isDropdown = "Dropdown".Equals(field.Type_CD);
        var hasPlaceHolder = !string.IsNullOrEmpty(field.Description_DESC);
        if (isDropdown || hasPlaceHolder)
        {
            sb.Append("[AeLabel(");

            if (isDropdown)
                sb.Append("isDropDown: true");

            if (hasPlaceHolder)
            {
                if (isDropdown)
                    sb.Append(", ");

                var placeholder = Quote($"[{field.Description_DESC}]");
                sb.Append($"placeholder: {placeholder}");
            }

            var validValues = GetValidValues(field.Choices_TXT);
            if (!string.IsNullOrEmpty(validValues))
            {
                sb.Append(", ").Append(validValues);
            }

            sb.Append(")]\n");
        }
    }

    private static string GetValidValues(string choices)
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

    private static string Quote(string value) => $"\"{value.Replace("\"", "\"")}\"";
}