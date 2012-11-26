using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using ED47.Stack.Reflector.Attributes;
using ED47.Stack.Reflector.Metadata;
using ED47.Stack.Reflector.Templates;

namespace ED47.Stack.Reflector
{
    public class ModelReflector
    {
        /// <summary>
        /// Generates the models in JavaScript.
        /// </summary>
        /// <param name="assemblyName">The fully qualitified assembly name.</param>
        /// <returns></returns>
        public static string GenerateModelScript(string assemblyName)
        {
            try
            {
                var assembly = Assembly.Load(assemblyName);
                var allTypes = assembly.GetTypes();
                var targetTypes = new List<Type>();
                
                foreach(var type in allTypes)
                {
                    var attributes = type.GetCustomAttributes(false);
                    for (var i = 0; i < attributes.Length; i++)
                    {
                        var modelAttribute = attributes[i] as ModelAttribute;
                        //TODO: Read more info from attribute in the future
                        if(modelAttribute != null)
                            targetTypes.Add(type);
                    }
                }

                var modelInfos = GetModelInfos(targetTypes);

                var codeBuilder = new StringBuilder();
                foreach (var modelInfo in modelInfos)
                {
                    var modelTemplate = new JsModel(modelInfo);
                    codeBuilder.Append(modelTemplate.TransformText());
                }

                return codeBuilder.ToString();
            }
            catch (System.IO.FileNotFoundException)
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Returns the ModelInfo collection from a list of types.
        /// </summary>
        /// <param name="targetTypes">The collection of types to get ModelInfo from.</param>
        /// <returns></returns>
        private static IEnumerable<ModelInfo> GetModelInfos(IEnumerable<Type> targetTypes)
        {
            var modelInfos = new List<ModelInfo>();
            foreach (var targetType in targetTypes)
            {
                var key = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(
                        el => el.GetCustomAttributes(typeof (KeyAttribute), true).Any());

                var modelInfo = new ModelInfo
                                    {
                                        Name = targetType.Name,
                                        ModelType = targetType,
                                        ModelProperties = GetModelProperties(targetType),
                                        IdPropertyName = key != null ? key.Name : "Id"
                                    };

                modelInfos.Add(modelInfo);
            }
            return modelInfos;
        }

        /// <summary>
        /// Gets the ModelPropertyInfo collection for all the properties of a Model.
        /// </summary>
        /// <param name="targetType">The Model target type.</param>
        /// <returns></returns>
        private static List<ModelPropertyInfo> GetModelProperties(Type targetType)
        {
            var modelPropertyInfos = new List<ModelPropertyInfo>();
            var properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            
            foreach(var property in properties)
            {
                var attributes = property.GetCustomAttributes(false);
                var modelPropertyInfo = new ModelPropertyInfo
                                            {
                                                Name = property.Name,
                                                PropertyInfo = property,
                                                IsRequired = (property.PropertyType == typeof(Int32)) || ModelReflectionHelper.CheckHasRequiredAttribute(attributes),
                                                Length = ModelReflectionHelper.GetLenghtAttribute(attributes),
                                                ExtXType = ModelReflectionHelper.GetExtXType(property, attributes),
                                                Label = ModelReflectionHelper.GetLabel(attributes) ?? property.Name,
                                                DropDownSource = ModelReflectionHelper.GetDropDownAttribute(attributes),
												Mapping = ModelReflectionHelper.GetMappingAttribute(attributes)
                                            };

                modelPropertyInfos.Add(modelPropertyInfo);
            }

            ModelReflector.ProcessDropDowns(modelPropertyInfos);

            return modelPropertyInfos;
        }

        /// <summary>
        /// Processes properties for linked dropdowns.
        /// </summary>
        /// <param name="modelPropertyInfos">The model property infos.</param>
        private static void ProcessDropDowns(List<ModelPropertyInfo> modelPropertyInfos)
        {
            var currentProperties = modelPropertyInfos.ToArray();
            foreach (var propertyInfo in currentProperties)
            {
                if (propertyInfo.DropDownSource != null)
                {
                    var dropdownSourceProperty =
                        modelPropertyInfos.SingleOrDefault(p => p.Name == propertyInfo.DropDownSource.DropDownPropertyName);
                    if (dropdownSourceProperty != null)
                    {
                        dropdownSourceProperty.DropDownForProperty = propertyInfo;
                        
                        if (dropdownSourceProperty.PropertyInfo.PropertyType.IsGenericType)
                        {
                            var dropdownType = dropdownSourceProperty.PropertyInfo.PropertyType.GetGenericArguments().First();

                            //Get the display field from the type's custom attributes
                            var displayAttribute = ModelReflectionHelper.GetDisplayColumnAttribute(dropdownType.GetCustomAttributes(inherit: true));
                            if (displayAttribute != null)
                                dropdownSourceProperty.DisplayField = displayAttribute.DisplayColumn;

                            //Get the value field from the property marked as key
                            dropdownSourceProperty.ValueField = ModelReflectionHelper.GetKeyFieldName(dropdownType);
                        }

                        propertyInfo.DropDownSourceProperty = dropdownSourceProperty;
                    }
                }
            }
        }
    }
}
