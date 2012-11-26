using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class ShowDropDownAttribute : Attribute
    {
        public string DropDownPropertyName { get; set; }
        public ShowDropDownAttribute(string dropdownDataPropertyName)
        {
            this.DropDownPropertyName = dropdownDataPropertyName;
        }
    }
}
