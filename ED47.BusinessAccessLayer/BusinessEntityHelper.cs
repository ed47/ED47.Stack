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
        public static UpdateResult Differences(this BusinessEntity original, BusinessEntity updatedObject)
        {
            var results = new UpdateResult();
            var type = original.GetType();
            var properties = type
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(el => el.Name != "ClientData" && !el.GetCustomAttributes(typeof(IgnoreDataMemberAttribute), true).Any());
            
            foreach (var propertyInfo in properties)
            {
                var originalValue = propertyInfo.GetValue(original, null);
                var updatedValue = propertyInfo.GetValue(updatedObject, null);

                if (Object.Equals(originalValue, updatedValue))
                    continue;

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