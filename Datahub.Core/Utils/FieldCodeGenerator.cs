using NRCan.Datahub.Shared.EFCore;
using System;
using System.Text;

namespace NRCan.Datahub.Shared.Utils
{
    public class FieldCodeGenerator
    {
        readonly Func<string, int> _sectionMapper;

        public FieldCodeGenerator(Func<string, int> sectionMapper)
        {
            _sectionMapper = sectionMapper;
        }

        public string GenerateJSON(WebForm_Field field)
        {
            return $"{Quote(field.SQLName)}: {Quote(System.Web.HttpUtility.JavaScriptStringEncode(field.Field_DESC))}";
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
                sb.AppendLine($"[AeFormCategory({ Quote(field.Section_DESC) }, { sectionIndex })]");
            }

            // AeLabel
            TryRenderLabel(sb, field);

            // EFCoreAnnotation1: field type
            if (FormFieldTypeReference.Annotations.ContainsKey(field.Type_CD))
            {
                sb.AppendLine(FormFieldTypeReference.Annotations[field.Type_CD]);
            }

            sb.AppendLine($"public {field.EFType} {field.SQLName} {{ get; set; }}");

            return sb.ToString();
        }

        static void TryRenderLabel(StringBuilder sb, WebForm_Field field)
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
                    sb.Append($"placeholder: { placeholder }");
                }
                sb.Append("]\n");
            }            
        }

        static string Quote(string value) => $"\"{ value.Replace("\"", "\"") }\"";
    }
}
