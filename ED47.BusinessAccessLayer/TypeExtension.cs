using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ED47.BusinessAccessLayer
{
    public static class TypeExtension
    {
        public static bool IsNullable(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
        }
    }
}
