using System;
using System.Linq;
using System.Reflection;

namespace NRCan.Datahub.Shared.Utils
{
    public static class ClassMapperExtension
    {
        public static object CopyPublicPropertiesTo(this object source, object dest, bool validate = false)
        {
            var sourceProps = source.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var destProps = dest.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var sourceProp in sourceProps)
            {
                var destProp = destProps.FirstOrDefault(p => p.CanWrite && p.Name == sourceProp.Name && p.PropertyType == sourceProp.PropertyType);
                
                if (validate && destProp == null)
                    throw new Exception($"Cannot find source property {sourceProp.Name} in destination instance!");

                destProp?.SetValue(dest, sourceProp.GetValue(source, null), null);
            }
            return dest;
        }
    }
}
