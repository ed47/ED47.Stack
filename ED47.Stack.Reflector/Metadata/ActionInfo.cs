using System;
using System.Net;
using System.Reflection;
using System.Web.Mvc;

namespace ED47.Stack.Reflector.Metadata
{
    public class ActionInfo
    {
        /// <summary>
        /// The action's parent controller.
        /// </summary>
        public ControllerInfo Controller { get; set; }

        /// <summary>
        /// The action's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The method info of this action.
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// The HTTP verb this method accepts.
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether [enable client caching].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [enable client caching]; otherwise, <c>false</c>.
        /// </value>
        public bool EnableClientCaching { get; set; }

        /// <summary>
        /// Gets the URL to call this action.
        /// </summary>
        /// <returns></returns>
        public string GetUrl()
        {
            var rootApiUrl = String.Empty;

            if (this.Controller.Kind == ControllerKind.Api)
            {
                rootApiUrl = Properties.Settings.Default.RootApiUrl;
                if (String.IsNullOrWhiteSpace(rootApiUrl))
                    rootApiUrl = "/api";

                if (!rootApiUrl.StartsWith("/"))
                    rootApiUrl = "/" + rootApiUrl;
                if (rootApiUrl.EndsWith("/"))
                    rootApiUrl = rootApiUrl.Substring(0, rootApiUrl.Length - 1);
            }

            return String.Format("{0}/{1}/{2}",
                                rootApiUrl,
                                this.Controller.ControllerType.Name.Replace("Controller", String.Empty), 
                                this.Name);
        }

        public static string GetHttpVerb(MethodInfo methodInfo)
        {
            var attributes = methodInfo.GetCustomAttributes(false);

            foreach (var attribute in attributes)
            {
                var getAttribute = attribute as HttpGetAttribute;
                if (getAttribute != null)
                    return WebRequestMethods.Http.Get;

                var postAttribute = attribute as HttpPostAttribute;
                if(postAttribute != null)
                    return WebRequestMethods.Http.Post;
            }
            
            return WebRequestMethods.Http.Post;
        }
    }
}
