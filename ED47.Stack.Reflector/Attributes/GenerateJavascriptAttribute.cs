using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Marks a MVC Web Controller or Action to be callable from Javascript. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class GenerateJavascriptAttribute : Attribute
    {
    }
}
