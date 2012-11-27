using System;
using System.Web;
using Newtonsoft.Json;

namespace ED47.Stack.Web.Multilingual
{
    /// <summary>
    /// Serves the translation file as JSON.
    /// </summary>
    public class MultilingualHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
// ReSharper disable RedundantAssignment
            var callAddKey = "return;";
// ReSharper restore RedundantAssignment

            #if DEBUG
            var addKey = context.Request["addkey"];

            if (!String.IsNullOrWhiteSpace(addKey))
            {
                Multilingual.AddMissingKey(addKey);
                return;
            }

            context.Response.Write("var addkey = function(key){ $.post('/MultilingualHandler.axd?addKey=' + key); };");
            callAddKey = "addkey(s);";
            #endif

            if (String.IsNullOrEmpty(context.Request["lang"]))
            {
                context.Response.Write("throw new Error('Especify a language via the [lang] parameter.');");
                return;
            }

            #if !DEBUG
            context.Response.Cache.SetExpires(DateTime.Now.AddMinutes(15));
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            #endif

            context.Response.ContentType = "text/javascript";
            context.Response.Write("var lang = $('html').attr('lang');var i8n = {}; i8n.n = function(s){ if(translations[s]) return translations[s].Text; else " + callAddKey + "};");

            context.Response.Write("var translations = ");
            context.Response.Write(JsonConvert.SerializeObject(Multilingual.GetLanguage(context.Request["lang"])));
            context.Response.Write(";");
            
            context.Response.Flush();
        }
    }
}
