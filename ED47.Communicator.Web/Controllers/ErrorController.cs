using System.Web.Mvc;

namespace ED47.Communicator.Web.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult PageNotFound()
        {
            return View();
        }
    }
}
