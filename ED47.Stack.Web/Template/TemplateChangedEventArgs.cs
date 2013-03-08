using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ED47.Stack.Web.Template
{
    public class TemplateChangedEventArgs : EventArgs
    {
        public string FileName { get; set; }
    }
}