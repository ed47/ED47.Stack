using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Reads and writes JSON data between a JSON entity field and business entity fields.
    /// </summary>
    public static class JsonDataFieldInjector
    {
        /// <summary>
        /// Reads JSON data from an entity.
        /// </summary>
        /// <param name="target">The target business entity.</param>
        /// <param name="source">The source entity containing the JSON data.</param>
        public static void ReadJsonData(this object target, object source)
        {
            var sourceType = source.GetType();

            var jsonProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(el => el.GetCustomAttributes(typeof(JsonDataFieldAttribute), false).Any())
                                .ToList();

            if (!jsonProperties.Any())
                return;

            if(jsonProperties.Count() > 1)
                throw new ApplicationException(String.Format("Entity '{0}' has more than one property marked with JsonDataFieldAttribute!", sourceType.Name));
            
            var json = jsonProperties.First().GetValue(source, null) as String;

            if (String.IsNullOrWhiteSpace(json))
                return;

            JsonConvert.PopulateObject(json, target);
        }

        /// <summary>
        /// Writes data from a business entity to an entity with a JSON field.
        /// </summary>
        /// <param name="target">The entity with a JSON field to write to.</param>
        /// <param name="source">The business entity with the fields that will be serialized to JSON.</param>
        public static void WriteJsonData(this object target, object source)
        {
            var sourceType = source.GetType();

            var jsonProperties = sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(el => el.GetCustomAttributes(typeof(JsonDataFieldAttribute), false).Any())
                                .ToList();
            
            if (!jsonProperties.Any())
                return;

            var json = new JObject();
            
            foreach (var propertyInfo in jsonProperties)
            {
                var value = propertyInfo.GetValue(source, null);
                json.Add(propertyInfo.Name, JToken.FromObject(value));
            }

            var targetType = target.GetType();
            var targetJsonProperty = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                        .Where(el => el.GetCustomAttributes(typeof(JsonDataFieldAttribute), false).Any())
                                        .ToList();

            if (!targetJsonProperty.Any())
                throw new ApplicationException(String.Format("BusinessEntity '{0}' has properties marked with JsonDataFieldAttribute but target Entity '{1}' has no target JSON property marked with JsonDataFieldAttribute!", sourceType.Name, targetType.Name));

            if (targetJsonProperty.Count() > 1)
                throw new ApplicationException(String.Format("Entity '{0}' has more than one property marked with JsonDataFieldAttribute!", sourceType.Name));

            targetJsonProperty.First().SetValue(target, json.ToString(Formatting.None), null);
        }
    }
}
