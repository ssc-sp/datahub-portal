using System.ComponentModel;
using System.Reflection;

namespace Datahub.Shared.Annotations;

public static class AnnotationTools
{
    /// <summary>
    /// Will get the string value for a given enums value, this will
    /// only work if you assign the StringValue attribute to
    /// the items in your enum.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Value of the Description annotation</returns>
    public static string GetStringValue(this Enum value)
    {
        // Get the type
        Type type = value.GetType();

        // Get fieldinfo for this type
        FieldInfo fieldInfo = type.GetField(value.ToString());

        // Get the stringvalue attributes
        DescriptionAttribute[] attribs = fieldInfo.GetCustomAttributes(
            typeof(DescriptionAttribute), false) as DescriptionAttribute[];

        // Return the first if there was a match.
        return attribs.Length > 0 ? attribs[0].Description : null;
    }
}