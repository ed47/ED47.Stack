using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Omu.ValueInjecter;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Injection from a dictionnary to a known type.
    /// </summary>
    public class DictionaryInjection : KnownSourceValueInjection<ICollection<KeyValuePair<string,object>>>
    {
        protected override void Inject(ICollection<KeyValuePair<string, object>> source, object target)
        {
            foreach (var prop in source)
            {
                var targetProp = target.GetProps().GetByName(prop.Key, true);
                if (targetProp == null || targetProp.IsReadOnly) continue;
                object value = prop.Value;

                if (value as JObject != null) continue;

                if(value != null && !targetProp.PropertyType.IsNullable() && targetProp.PropertyType !=value.GetType() && (targetProp.PropertyType.IsPrimitive || targetProp.PropertyType == typeof(decimal)))
                {
                    value = Convert.ChangeType(value, targetProp.PropertyType);
                }

                if (targetProp.PropertyType.IsNullable() && (value != null && Nullable.GetUnderlyingType(targetProp.PropertyType) != value.GetType()))
                {
                    value = Convert.ChangeType(value, Nullable.GetUnderlyingType(targetProp.PropertyType));
                }

                if (value == String.Empty && targetProp.PropertyType != typeof(String))
                    value = null;

                if (value is JArray)
                {
                    var jArray = (JArray)value;
                    
                    if (jArray.Any())
                    {
                        if (jArray.First().Type == JTokenType.Integer)
                            value = jArray.Select(el => (int)el).ToArray();

                        if (jArray.First().Type == JTokenType.String)
                            value = jArray.Select(el => (string)el).ToArray();
                    }
                }

                targetProp.SetValue(target, value);
            }
        }
    }

}
