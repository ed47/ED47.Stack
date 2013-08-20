using System;
using System.Collections.Generic;

using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentValidation.Results;

namespace ED47.Stack.Web
{
    public static class ValidationResultExtension
    {
        public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
        {
            foreach (var error in result.Errors)
            {
                modelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

    }
}