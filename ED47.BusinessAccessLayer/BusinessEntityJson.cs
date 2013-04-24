using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace ED47.BusinessAccessLayer
{
    public static class BusinessEntityJson
    {
        public static JObject JsonDifferences(this BusinessEntity original, BusinessEntity updatedObject)
        {
            var json = new JObject();
            var type = original.GetType();
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(el => el.Name != "ClientData" && !el.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Any());

            foreach (var propertyInfo in properties)
            {
                var originalValue = propertyInfo.GetValue(original, null);
                var updatedValue = propertyInfo.GetValue(updatedObject, null);
                JToken JValue;

                if (Object.Equals(originalValue, updatedValue))
                    continue;

                if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.IsInterface &&
                    propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    JValue = new JArray(updatedValue);
                }
                else
                {
                    if (Object.Equals(originalValue, updatedValue))
                        continue;

                    JValue = new JValue(updatedValue);
                }

                json.Add(propertyInfo.Name, JValue);
            }

            return json;
        }
    }
}