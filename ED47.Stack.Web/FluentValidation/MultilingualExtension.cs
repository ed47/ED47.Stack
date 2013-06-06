using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;

namespace ED47.Stack.Web.FluentValidation
{
    public static class MultilingualExtension
    {
        public static IRuleBuilderOptions<T, TProperty> WithMultilingualMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, string key, params object[] parameters)
        {
            return rule.Configure(config =>
            {
                config.CurrentValidator.ErrorMessageSource = new MultilingualValidationSource(key, parameters);
            });
        }
    }
}