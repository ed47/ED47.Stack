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
            context.Response.Write(cachedStaticFilesResult);

            var assemblyNames = context.Request.QueryString["assemblyName"];
            
            if (String.IsNullOrWhiteSpace(assemblyNames))
            {
                context.Response.ContentType = "text/javascript";
                context.Response.Flush();
                return;
            }

            var assemblyNameArray = assemblyNames.Split(';');

            foreach (var assemblyName in assemblyNameArray)
            {
                var cacheKey = "ED47.Stack.Reflector?assemblyName=" + assemblyName;
                // ReSharper disable RedundantAssignment
                var cachedResult = MemoryCache.Default.Get(cacheKey) as string;
                // ReSharper restore RedundantAssignment
#if DEBUG
                //Disable caching during development to make debugging easier
                cachedResult = null;
#endif
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                if (cachedResult == null)
                // ReSharper restore ConditionIsAlwaysTrueOrFalse
                {
                    cachedResult = ApiReflector.GenerateControllerScript(assemblyName) + ModelReflector.GenerateModelScript(assemblyName);
                    MemoryCache.Default.Add(new CacheItem(cacheKey, cachedResult),
                                            new CacheItemPolicy {Priority = CacheItemPriority.NotRemovable});
                }

                context.Response.Write(cachedResult);
            }
            
            context.Response.ContentType = "text/javascript";
            context.Response.Flush();
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
