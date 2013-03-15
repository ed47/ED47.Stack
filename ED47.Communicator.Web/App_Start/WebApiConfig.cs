using System.Web.Http;

namespace ED47.Communicator.Web.App_Start
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "apiREST/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
