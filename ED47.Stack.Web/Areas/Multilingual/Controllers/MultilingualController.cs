using System;
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
            ViewBag.Root = root;
            ViewBag.Language = language;

            return View();
        }

        public ActionResult Translations(string root = null, string language = "en", string search = null)
        {
            var dict = Web.Multilingual.Multilingual.GetLanguage(language);
            IEnumerable<ITranslationEntry> res = null;

            if (!String.IsNullOrWhiteSpace(root) && !String.IsNullOrWhiteSpace(search))
            {
                search = search.ToLowerInvariant();
                res = dict.Where(el => el.Key.StartsWith(root) && (el.Key.ToLowerInvariant().Contains(search) || el.Value.Value.ToLowerInvariant().Contains(search))).Select(el => el.Value);
            }

            return PartialView("_Translations", res ?? dict.Values);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(string key, string value, string fileName)
        {
            var allowedRoles = System.Configuration.ConfigurationManager.AppSettings["MultilingualEditAllowedRole"];
            if (allowedRoles != null && !User.IsInRole(allowedRoles))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            Web.Multilingual.Multilingual.UpdateEntry(ED47.Stack.Web.Properties.Settings.Default.DefaultLanguage, key, value, fileName);

            if (Request.IsAjaxRequest())
                return new EmptyResult();

            return RedirectToAction("Index");
        }
    }
}
