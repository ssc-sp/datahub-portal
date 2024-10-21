using Elemental.Components;
using Microsoft.Extensions.Logging;

namespace Datahub.Core.Services.Data;

public class DataImportingService
{
    private ILogger<DataImportingService> _logger;

    public DataImportingService(ILogger<DataImportingService> logger)
    {
        _logger = logger;
    }

    public List<T> CreateObjects<T>(List<string[]> contents)
    {
        List<T> objectList = new List<T>();

        foreach (var item in contents)
        {
            var properties = typeof(T).GetProperties().Where(p => !Attribute.IsDefined(p, typeof(AeFormIgnoreAttribute))).ToList();

            //Check to make sure array length will fit into the object
            if (item.Count() == properties.Count())
            {
                T obj = (T)Activator.CreateInstance(typeof(T));

                int index = 0;
                foreach (var prop in properties)
                {
                    //check to make sure the types in the array match the object
                    //in prod this has to be a bit more subtle
                    var propertyName = prop.Name;
                    var propertyType = prop.PropertyType;
                    var arrayValue = item[index];
                    var objectproperty = obj.GetType().GetProperty(propertyName);

                    if (objectproperty.CanWrite)
                    {
                        if (propertyType == typeof(int?)
                            || propertyType == typeof(int)
                            || propertyType == typeof(double?)
                            || propertyType == typeof(double))
                        {
                            arrayValue = arrayValue.Replace(",", string.Empty);
                            if (propertyType == typeof(double) || propertyType == typeof(double?))
                            {
                                double outValue;
                                if (double.TryParse(arrayValue, out outValue))
                                {
                                    if (propertyType == typeof(double?))
                                    {
                                        objectproperty.SetValue(obj, outValue);
                                    }
                                    else
                                    {
                                        objectproperty.SetValue(obj, outValue);
                                    }
                                }
                                else
                                {
                                    objectproperty.SetValue(obj, 0.0);
                                }
                            }
                            else
                            {
                                int outValue;
                                if (int.TryParse(arrayValue, out outValue))
                                {
                                    if (propertyType == typeof(int?))
                                    {
                                        objectproperty.SetValue(obj, outValue);
                                    }
                                    else
                                    {
                                        objectproperty.SetValue(obj, outValue);
                                    }
                                }
                                else
                                {
                                    objectproperty.SetValue(obj, 0);
                                }
                            }
                        }
                        else //strings
                        {
                            objectproperty.SetValue(obj, arrayValue.ToString());
                        }
                    }
                    index++;
                }

                objectList.Add(obj);
            }
        }

        return objectList;
    }
}