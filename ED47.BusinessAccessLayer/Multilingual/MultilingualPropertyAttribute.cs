using System;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Marks a property as multilingual. Multilingual entity is necessary to store and read multilingual values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class MultilingualPropertyAttribute : Attribute
    {
    }
}
