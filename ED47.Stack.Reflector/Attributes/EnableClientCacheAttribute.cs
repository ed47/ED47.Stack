using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.Stack.Reflector.Attributes
{
    /// <summary>
    /// Enables calling an action without caching busting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class EnableClientCacheAttribute : Attribute
    {
    }
}
