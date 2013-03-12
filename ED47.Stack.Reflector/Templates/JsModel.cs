using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ED47.Stack.Reflector.Metadata;

namespace ED47.Stack.Reflector.Templates
{
    public partial class JsModel
    {
        public JsModel(ModelInfo modelInfo)
        {
            this.ModelInfo = modelInfo;
        }

        public ModelInfo ModelInfo { get; set; }
    }
}
