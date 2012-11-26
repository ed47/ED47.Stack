using System;
using System.Linq;
using System.Web.Mvc;
using i18n = ED47.Stack.Web.Multilingual.Multilingual;

namespace ED47.Stack.Web.Multilingual.Areas.Multilingual.Controllers
{
    public class MultilingualController : Controller
    {
        public ActionResult Index(string root = null, string language = "en")
        {
            var model = i18n.GetLanguage(language);

            if (!String.IsNullOrWhiteSpace(root))
                model = model.Where(el => el.Key.StartsWith(root)).ToDictionary(el => el.Key, el => el.Value);

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(string key, string value, string fileName)
        {
            i18n.UpdateEntry(key, value, fileName);

            if (Request.IsAjaxRequest())
                return new EmptyResult();

            return RedirectToAction("Index");
        }
    }
}
