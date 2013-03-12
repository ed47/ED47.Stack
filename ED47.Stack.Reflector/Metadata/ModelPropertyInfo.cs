using System.ComponentModel.DataAnnotations;
using System.Reflection;
using ED47.Stack.Reflector.Attributes;

namespace ED47.Stack.Reflector.Metadata
{
    public class ModelPropertyInfo
    {
        public string Name { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public bool IsRequired { get; set; }

        public StringLengthAttribute Length { get; set; }

        public string ExtXType { get; set; }

        public string Label { get; set; }

        public ShowDropDownAttribute DropDownSource { get; set; }

        public ModelPropertyInfo DropDownForProperty { get; set; }

        public ModelPropertyInfo DropDownSourceProperty { get; set; }

        public string DisplayField { get; set; }

        public string ValueField { get; set; }
        
        public string Mapping { get; set; }
    }
}