using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
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

            var addKey = context.Request["addkey"];
            if (!String.IsNullOrWhiteSpace(addKey))
            {
                if (Multilingual.Repository.DefaultDictionnary.AutoAddEntry)
                {
                    Multilingual.AddMissingKey(addKey);
                    return;
                }
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return;
            }

            var lan = context.Request["lang"];
            var cache = Multilingual.Repository.GetFromCache("MultilingualHandler?lan=" + lan);
            if (string.IsNullOrEmpty(cache))
            {

                context.Response.ContentType = "text/javascript";
                var httpResponse = new StringBuilder();

                httpResponse.AppendLine(
                    "var addkey = function(key){ $.post('/MultilingualHandler.axd?addKey=' + key); };");
                callAddKey = "addkey(s);";


                if (String.IsNullOrEmpty(context.Request["lang"]))
                {
                    httpResponse.AppendLine("throw new Error('Especify a language via the [lang] parameter.');");
                    return;
                }


                httpResponse.AppendLine(
                    "var lang = $('html').attr('lang');var i8n = {}; i8n.n = function(s){ if(translations[s]) return translations[s]; else " +
                    callAddKey + "};");


                httpResponse.AppendLine("var translations = ");



                var allKeys = Multilingual.GetAllKeys();
                var dict = new Dictionary<string, string>();
                foreach (var k in allKeys)
                {
                    dict.Add(k, Multilingual.N2(k, lan));
                }
                httpResponse.AppendLine(JsonConvert.SerializeObject(dict));
                httpResponse.AppendLine(";");
                cache = httpResponse.ToString();

                Multilingual.Repository.CacheData("MultilingualHandler?lan=" + lan, cache, lan);

            }

            context.Response.Write(cache);


        }
    }
}
