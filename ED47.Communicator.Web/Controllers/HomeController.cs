using System;
using System.Web.Mvc;
using ED47.Communicator.Shared.BusinessEntities;

namespace ED47.Communicator.Web.Controllers
{
    public class HomeController : Controller
    {

        

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Test(string code = "")
        {
            var doc = Category.Get(code) ?? new Category { Name = DateTime.Now.ToShortDateString(), Code = code, Data = new int[]{1,2,3}};
            
            doc.Save();
            return View("Index");
        }
    }
}
