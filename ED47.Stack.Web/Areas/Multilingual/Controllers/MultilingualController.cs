using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ED47.Stack.Web.Multilingual;

namespace ED47.Stack.Web.Areas.Multilingual.Controllers
{
    public class MultilingualController : Controller
    {
        public ActionResult Index(string root = null, string language = "en")
        {
            var dict = Web.Multilingual.Multilingual.GetLanguage(language);

            IEnumerable<TranslationItem> res = null;

            if (!String.IsNullOrWhiteSpace(root))
                res = dict.Where(el => el.Key.StartsWith(root)).Select(el=>el.Value);

            return View(res ?? dict.Values);
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
