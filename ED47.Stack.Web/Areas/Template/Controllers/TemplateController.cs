using System.Net;
using System.Web.Mvc;
using ED47.Stack.Web.Template;

namespace ED47.Stack.Web.Areas.Template.Controllers
{
    public class TemplateController : Controller
    {
        private static readonly object WriteFileLock = new object();

        public ActionResult Index()
        {
            return View(Web.Template.Template.Templates);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(string templateText, string name)
        {
            var allowedRoles = System.Configuration.ConfigurationManager.AppSettings["TemplateEditAllowedRole"];
            if(allowedRoles != null && !User.IsInRole(allowedRoles))
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);

            lock (WriteFileLock)
            {
                var template = Web.Template.Template.Get(name);
                
                //Save to file
                System.IO.File.WriteAllText(template.Name, templateText);

                template.OnChanged(new TemplateChangedEventArgs { FileName = template.Name });
            }

            if(Request.IsAjaxRequest())
                return new EmptyResult();
            
            return RedirectToAction("Index");
        }
    }
}
