using System.Web.Mvc;
using MvcContrib.PortableAreas;

namespace ED47.Stack.Web.Multilingual.Areas.Multilingual
{
    public class MultilingualAreaRegistration : PortableAreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Multilingual";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context, IApplicationBus bus)
        {
            bus.Send(new PortableAreaStartupMessage(AreaName));

            context.MapRoute(
                AreaName + "_default",
                base.AreaRoutePrefix + "/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "ED47.Stack.Web.Multilingual.Areas.Multilingual.Controllers", "MvcContrib" }
            );

            RegisterAreaEmbeddedResources();
        }
    }
}
