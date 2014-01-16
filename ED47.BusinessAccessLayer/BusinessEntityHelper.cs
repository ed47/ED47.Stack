using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using FluentValidation.Results;

namespace ED47.BusinessAccessLayer
{
    public static class BusinessEntityHelper
    {
        public static UpdateResult Differences(this IBusinessEntity original, IBusinessEntity updatedObject)
        {
            var results = new UpdateResult();
            var type = original.GetType();
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(el => !el.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Any());
            
            foreach (var propertyInfo in properties)
            {
                var originalValue = propertyInfo.GetValue(original, null);
                var updatedValue = propertyInfo.GetValue(updatedObject, null);

                if (propertyInfo.PropertyType.IsInterface && propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof (ICollection<>))
                {

                    var sequenceEqualMethodInfo = typeof(Enumerable)
                                                        .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                        .First(el => el.Name == "SequenceEqual" && el.GetParameters().Count() == 2);
                    sequenceEqualMethodInfo = sequenceEqualMethodInfo.MakeGenericMethod(propertyInfo.PropertyType.GetGenericArguments());
                    var areEqual = (bool)sequenceEqualMethodInfo.Invoke(null, new[] { originalValue, updatedValue });

                    if(areEqual)
                        continue;
                }
                else
                {
                    if (Object.Equals(originalValue, updatedValue))
                        continue;
                }
                
                results.Values.Add(propertyInfo.Name, updatedValue);
            }

            return results;
        }
    }
    
    public class UpdateResult
    {
        public Dictionary<string, object> Values { get; set; }
        public IEnumerable<FluentValidation.Results.ValidationFailure> Validations { get; set; }

        public UpdateResult()
        {
            Values = new Dictionary<string, object>();
            Validations = new List<ValidationFailure>();
        }
    }
}