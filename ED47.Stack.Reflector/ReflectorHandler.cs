using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Caching;
using System.Text;
using System.Web;

namespace ED47.Stack.Reflector
{
    /// <summary>
    /// HTTP Handler that provides the generated JavaScript.
    /// It must be added to the handlers in the ASP.Net MVC projects that will use the ED47 Stack.
    /// </summary>
    /// <example>
    ///     Add under the <handlers></handlers> tag in the web.config:
    ///     <add name="ReflectorHandler" path="ED47StackJS.axd" type="ED47.Stack.Reflector.ReflectorHandler" verb="GET" />
    /// </example>
    public class ReflectorHandler : IHttpHandler
    {
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var js = new StringBuilder();
#if !DEBUG
            var minifier = new Microsoft.Ajax.Utilities.Minifier();
#endif
            context.Response.ContentType = "text/javascript";
            
            // ReSharper disable RedundantAssignment
            var cachedStaticFilesResult = MemoryCache.Default.Get("ED47.Stack.Reflector.Static") as string;
            // ReSharper restore RedundantAssignment
#if DEBUG
            //Disable caching during development to make debugging easier
            cachedStaticFilesResult = null;
#endif
            // ReSharper disable ConditionIsAlwaysTrueOrFalse
            if (cachedStaticFilesResult == null)
            // ReSharper restore ConditionIsAlwaysTrueOrFalse
            {
                var staticScriptBuilder = new StringBuilder();
                staticScriptBuilder.AppendLine("/*==================  STATIC SCRIPTS ==================*/");
                ReflectorHandler.AppendStaticScripts(staticScriptBuilder);
                staticScriptBuilder.AppendLine("/*=====================================================*/");
                cachedStaticFilesResult = staticScriptBuilder.ToString();
            }
            
            js.Append(cachedStaticFilesResult);

            var assemblyNames = context.Request.QueryString["assemblyName"];
            
            if (String.IsNullOrWhiteSpace(assemblyNames))
            {
                context.Response.ContentType = "text/javascript";
                #if !DEBUG
                    context.Response.Write(minifier.MinifyJavaScript(js.ToString()));
                #else
                    context.Response.Write(js.ToString());
                #endif
                return;
            }

            var assemblyNameArray = assemblyNames.Split(new []{";"}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var assemblyName in assemblyNameArray)
            {
                var cacheKey = "ED47.Stack.Reflector?assemblyName=" + assemblyName;
                // ReSharper disable RedundantAssignment
                var cachedResult = MemoryCache.Default.Get(cacheKey) as string;
                // ReSharper restore RedundantAssignment
#if DEBUG
                cachedResult = null; //Disable caching during development to make debugging easier
#endif
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (cachedResult == null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    cachedResult = ApiReflector.GenerateControllerScript(assemblyName) + ModelReflector.GenerateModelScript(assemblyName);
                    MemoryCache.Default.Add(new CacheItem(cacheKey, cachedResult),
                                            new CacheItemPolicy {Priority = CacheItemPriority.NotRemovable});
                }
                js.Append(cachedResult);
            }
            context.Response.Cache.SetCacheability(HttpCacheability.Public);
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(1));
            
#if DEBUG
            context.Response.Write(js.ToString());
#else
            context.Response.Write(minifier.MinifyJavaScript(js.ToString()));
#endif
        }

        /// <summary>
        /// Appends all static scripts in the /Scripts folder of the project.
        /// </summary>
        /// <remarks>Make sure scripts have Build Action set to "Embeded Ressource".</remarks>
        /// <param name="builder">The StringBuilder to append to.</param>
        private static void AppendStaticScripts(StringBuilder builder)
        {
            //Add static scripts
            var currentAssembly = Assembly.GetExecutingAssembly();
            var staticScripts = currentAssembly.GetManifestResourceNames()
                                    .Where(n => n.Contains(".Scripts."));

            foreach (var scriptName in staticScripts)
            {
                using (var stream = currentAssembly.GetManifestResourceStream(scriptName))
                {
                    if (stream == null) continue;

                    using (var streamReader = new StreamReader(stream))
                    {
                        builder.AppendLine(streamReader.ReadToEnd());
                    }
                }
            }
        }
    }
}
