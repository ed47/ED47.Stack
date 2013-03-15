using System;

namespace ED47.BusinessAccessLayer.Couchbase
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DocumentAttribute : Attribute
    {
        public static DocumentAttribute Get(Type documentType)
        {
            return GetCustomAttribute(documentType, typeof (DocumentAttribute)) as DocumentAttribute;
        }

        public string Name;
    }
}