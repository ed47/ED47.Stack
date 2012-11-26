using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web.Mvc;
using Newtonsoft.Json;
namespace ED47.Stack.Web.HelperTemplates
{
    [Serializable]
    public class ClientControlModel 
    {
        public string Id { get; set; }
        [IgnoreDataMember]
        public string FullName { get; set; }
        public string StoreId { get; set; }
        public dynamic Config { get; set; }
        /// <summary>
        /// Render the script used to initialize a client control in a razor view.
        /// </summary>
        public string RenderControlScript()
        {
            var tpl = Template.Get("ED47.Stack.Web.HelperTemplates.ClientControlScript.cshtml");
            if (tpl == null) return (String.Empty);

            return (tpl.Apply(this));
        }
    }
}
