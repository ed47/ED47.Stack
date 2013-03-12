using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Marks a property as multilingual. Multilingual entity is necessary to store and read multilingual values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MultilingualPropertyAttribute : Attribute
    {
    }
}
