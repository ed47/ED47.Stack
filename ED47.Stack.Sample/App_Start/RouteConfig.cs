using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace ED47.Stack.Sample
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            //Route added for calling WebAPI actions non-RESTfully (required by ED47.Stack).
            var apiRoute = routes.MapHttpRoute(
                name: "ReflectorApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
            apiRoute.DataTokens = new RouteValueDictionary { { "Namespaces", new[]
                {
                    "ED47.Stack.Sample.ApiControllers" //TODO: Change me!
                } } };
        }
    }
}