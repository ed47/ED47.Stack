using System;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Marks an entity property as containing JSON data that will be mapped to BusinessEntity properties or a BusinessEntity properties that will be saved in JSON.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class JsonDataFieldAttribute : Attribute
    {
    }
}
