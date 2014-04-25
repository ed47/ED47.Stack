using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json.Linq;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Injection from a dictionnary to a known type.
    /// </summary>
    public class DictionaryInjection : KnownSourceValueInjection<ICollection<KeyValuePair<string, object>>>
    {
        protected override void Inject(ICollection<KeyValuePair<string, object>> source, object target)
        {
            foreach (var prop in source)
            {
                var targetProp = target.GetProps().GetByName(prop.Key, true);
                if (targetProp == null || targetProp.IsReadOnly) continue;
                object value = prop.Value;

                if (value as JObject != null) continue;

                if (value != null && !targetProp.PropertyType.IsNullable() && targetProp.PropertyType != value.GetType() && (targetProp.PropertyType.IsPrimitive || targetProp.PropertyType == typeof(decimal)))
                {
                    value = Convert.ChangeType(value, targetProp.PropertyType);
                }

                value = ConvertNullable(targetProp, value);

                if (value == String.Empty && targetProp.PropertyType != typeof(String))
                    value = null;

                value = ReadJArray(value);

                SetValue(target, targetProp, value);
            }
        }

        private static void SetValue(object target, PropertyDescriptor targetProp, object value)
        {
            if (targetProp.PropertyType.IsGenericType && targetProp.PropertyType.IsInterface &&
                targetProp.PropertyType.GetGenericTypeDefinition() == typeof (ICollection<>))
            {
                if (targetProp.GetValue(target) == null)
                {
                    var collection = typeof (Collection<>);
                    var typeArgs = targetProp.PropertyType.GetGenericArguments();
                    targetProp.SetValue(target, Activator.CreateInstance(collection.MakeGenericType(typeArgs)));
                }

                if (value == null)
                    return;

                var values = ((string) value).Split(',');
                var collectionType = targetProp.PropertyType.GetGenericArguments().First();

                foreach (var collectionValue in values)
                {
                    object convertedValue = collectionValue;

                    if (collectionType != typeof (string))
                        convertedValue = Convert.ChangeType(convertedValue, collectionType);

                    targetProp.PropertyType.GetMethod("Add").Invoke(targetProp.GetValue(target), new object[] {convertedValue});
                }

                return;
            }

            targetProp.SetValue(target, value);
        }

        private static object ReadJArray(object value)
        {
            if (value is JArray)
            {
                var jArray = (JArray) value;

                if (jArray.Any())
                {
                    if (jArray.First().Type == JTokenType.Integer)
                        value = jArray.Select(el => (int) el).ToArray();

                    if (jArray.First().Type == JTokenType.String)
                        value = jArray.Select(el => (string) el).ToArray();
                }
            }
            return value;
        }

        private static object ConvertNullable(PropertyDescriptor targetProp, object value)
        {
            if (targetProp.PropertyType.IsNullable() &&
                (value != null && Nullable.GetUnderlyingType(targetProp.PropertyType) != value.GetType()))
            {
                if (value.GetType().Name == "String" && String.IsNullOrWhiteSpace(value.ToString()))
                    value = null;
                else
                {
                    if (targetProp.PropertyType.BaseType == typeof (Enum))
                        value = Enum.Parse(targetProp.PropertyType, (string) value);
                    else if (targetProp.PropertyType.IsGenericType &&
                             targetProp.PropertyType.GetGenericArguments().First().BaseType == typeof (Enum))
                        value = Enum.Parse(targetProp.PropertyType.GetGenericArguments().First(), (string) value);
                    else
                        value = Convert.ChangeType(value, Nullable.GetUnderlyingType(targetProp.PropertyType));
                }
            }
            return value;
        }
    }

}
