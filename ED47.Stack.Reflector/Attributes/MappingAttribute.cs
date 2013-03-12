using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Marks a property as Mapping property to another property. Mapping is necessary to TreeStore Model values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class MappingAttribute : Attribute
    {
        public string PropertyName { get; set; }
    }
}
