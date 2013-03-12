using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Attribute indicated that a class, method, property or class will be ignored for Javascript generation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false,Inherited = false)]
    public class SkipJavascriptGenerationAttribute : Attribute
    {
    }
}
