﻿using System;
using FluentValidation.Resources;

namespace ED47.Stack.Web.FluentValidation
{
    public class i18nValidationSource : IStringSource
    {
        public i18nValidationSource(string key)
        {
            this.TranslationKey = key;
        }

        /// <summary>
        /// Construct the error message template
        /// </summary>
        /// <returns>
        /// Error message template
        /// </returns>
        public string GetString()
        {
            return Multilingual.Multilingual.N(this.TranslationKey);
        }

        /// <summary>
        /// The name of the resource if localized.
        /// </summary>
        public string ResourceName { get; private set; }

        /// <summary>
        /// The type of the resource provider if localized.
        /// </summary>
        public Type ResourceType { get; private set; }

        public string TranslationKey { get; private set; }
    }
}