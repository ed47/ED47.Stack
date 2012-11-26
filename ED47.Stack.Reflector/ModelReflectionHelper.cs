using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using ED47.Stack.Reflector.Attributes;

namespace ED47.Stack.Reflector
{
    public static class ModelReflectionHelper
    {
        /// <summary>
        /// Checks if a collection of custom attributes contains the [Required] attribute.
        /// </summary>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static bool CheckHasRequiredAttribute(object[] attributes)
        {
            return attributes.OfType<RequiredAttribute>().Any();
        }

        /// <summary>
        /// Gets the String Length attribute from a list of custom attributes.
        /// </summary>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static StringLengthAttribute GetLenghtAttribute(object[] attributes)
        {
            var stringLengthAttributes = attributes.OfType<StringLengthAttribute>().ToList();
            if(stringLengthAttributes.Any())
                return stringLengthAttributes.First();
            
            var maxLengthAttributes = attributes.OfType<MaxLengthAttribute>().ToList();
            var minLengthAttributes = attributes.OfType<MinLengthAttribute>().ToList();
            if (maxLengthAttributes.Any())
            {
                var stringLength = new StringLengthAttribute(maxLengthAttributes.First().Length);

                if (minLengthAttributes.Any())
                    stringLength.MinimumLength = minLengthAttributes.First().Length;

                return stringLength;
            }

            return null;
        }


        /// <summary>
        /// Gets the mappgin field attribute from a list of custom attributes.
        /// </summary>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static string GetMappingAttribute(object[] attributes) {
            var mappingAttributes = attributes.OfType<MappingAttribute>();
            if(mappingAttributes.Any())
                return mappingAttributes.FirstOrDefault().PropertyName;

            return null;
        }

        /// <summary>
        /// Gets the ExtJS XType for the property from it's custom attributes.
        /// </summary>
        /// <param name="propertyInfo">The model property's type.</param>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static string GetExtXType(PropertyInfo propertyInfo, object[] attributes)
        {
            if (propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                return ExtJsXTypes.combo.ToString();
            }

            var dropdownAttribute = ModelReflectionHelper.GetDropDownAttribute(attributes);
            if (dropdownAttribute != null)
                return ExtJsXTypes.combo.ToString();

            return ExtJsXTypes.textfield.ToString();
        }

        /// <summary>
        /// Gets the label of a model property.
        /// </summary>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static string GetLabel(object[] attributes)
        {
            var displayAttribute = attributes.OfType<DisplayAttribute>().FirstOrDefault();
            if (displayAttribute != null)
                return displayAttribute.GetName();
            return null;
        }

        /// <summary>
        /// Gets the ShowDropDownAttribute of the passed property.
        /// </summary>
        /// <param name="attributes">The collection of custom attributes.</param>
        /// <returns></returns>
        public static ShowDropDownAttribute GetDropDownAttribute(object[] attributes)
        {
            return attributes.OfType<ShowDropDownAttribute>().FirstOrDefault();
        }

        /// <summary>
        /// Gets the DisplayColumnAttribute for a class.
        /// </summary>
        /// <param name="attributes">The classe's collection of custom attributes.</param>
        /// <returns></returns>
        public static DisplayColumnAttribute GetDisplayColumnAttribute(object[] attributes)
        {
            return attributes.OfType<DisplayColumnAttribute>().SingleOrDefault();
        }

        public static string GetKeyFieldName(Type type)
        {
            var property = type.GetProperties()
                                .FirstOrDefault(p => p.GetCustomAttributes(true).OfType<KeyAttribute>().Any());

            if (property != null)
                return property.Name;

            return null;
        }
    }
}
