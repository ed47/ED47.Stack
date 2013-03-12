using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Indicates Stack Reflector to take the penultimate namespace of the controller and add it to the Javascript namespace. 
    /// </summary>
    /// <example>ED47.GrcTool.Front.ApiControllers will have "Front" prefixed to the controller's name.</example>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateWithNamespaceAttribute : Attribute
    {
    }
}
