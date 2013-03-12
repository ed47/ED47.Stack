using System;
using System.Collections.Generic;

namespace ED47.Stack.Reflector.Metadata
{
    public class ModelInfo
    {
        public string Name { get; set; }

        public Type ModelType { get; set; }

        public List<ModelPropertyInfo> ModelProperties { get; set; }

        public string IdPropertyName { get; set; }
    }
}
