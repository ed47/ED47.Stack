using System.Web.Mvc;

namespace ED47.Stack.Sample.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}
