using System;
using System.Linq;

namespace ED47.Stack.Web
{
    public static class Utilities
    {
        /// <summary>
        /// Returns the name of the type.
        /// </summary>
        /// <param name="type">The Type to get the name from.</param>
        /// <returns></returns>
        public static string GetTypeName(Type type)
        {
            return !type.IsGenericType ? type.Name : type.GetGenericArguments().First().Name;
        }


    }
}