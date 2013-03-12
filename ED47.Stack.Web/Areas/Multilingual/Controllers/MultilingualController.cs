using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ED47.Stack.Web.Areas.Multilingual.Controllers
{
    public class MultilingualController : Controller
    {
        public ActionResult Index(string root = null, string language = "en")
        {
            var model = Web.Multilingual.Multilingual.GetLanguage(language);

            if (!String.IsNullOrWhiteSpace(root))
                model = model.Where(el => el.Key.StartsWith(root)).ToDictionary(el => el.Key, el => el.Value);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(string key, string value, string fileName)
        {
            var allowedRoles = System.Configuration.ConfigurationManager.AppSettings["MultilingualEditAllowedRole"];
            if (allowedRoles != null && !User.IsInRole(allowedRoles))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            Web.Multilingual.Multilingual.UpdateEntry(key, value, fileName);

            if (Request.IsAjaxRequest())
                return new EmptyResult();

            return RedirectToAction("Index");
        }
    }
}
