using System;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;

namespace ED47.Stack.Web.FluentValidation
{
    public static class MultilingualExtension
    {
        public static IRuleBuilderOptions<T, TProperty> WithMultilingualMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, string key, params Func<T, object>[] funcs)
        {
            return rule.Configure(config =>
            {
                config.CurrentValidator.ErrorMessageSource = new MultilingualValidationSource(key);
                var validator = funcs.Select(func => Extensions.CoerceToNonGeneric<T, object>(func));

                foreach (var func in validator)
                {
                    config.CurrentValidator.CustomMessageFormatArguments.Add(func);
                }
            });
        }
    }
}