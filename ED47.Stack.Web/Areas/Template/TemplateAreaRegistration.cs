using System.Web.Mvc;
using MvcContrib.PortableAreas;

namespace ED47.Stack.Web.Areas.Template
{
    public class TemplateAreaRegistration : PortableAreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Template";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context, IApplicationBus bus)
        {
            bus.Send(new PortableAreaStartupMessage(AreaName));

            context.MapRoute(
                AreaName + "_default",
                base.AreaRoutePrefix + "/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "ED47.Stack.Web.Areas.Template.Controllers", "MvcContrib" }
            );

            RegisterAreaEmbeddedResources();
        }
    }
}
