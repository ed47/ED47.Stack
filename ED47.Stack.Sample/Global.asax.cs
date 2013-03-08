using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using FluentValidation.Mvc;

namespace ED47.Stack.Sample
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            //We never use XML in WebAPI, feel free to remove this if you need it
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();

            //Register fluent validations
            FluentValidationModelValidatorProvider.Configure();
        }
    }
}