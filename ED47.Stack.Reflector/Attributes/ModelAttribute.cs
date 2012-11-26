using System;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Custom attribute to mark models that should be generated in JavaScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ModelAttribute : Attribute
    {
    }
}
