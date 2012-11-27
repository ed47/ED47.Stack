using System.Net;
using System.Web.Mvc;

//using File = ED47.BusinessAccessLayer.BusinessEntities.File;

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

                //Save to repository for versioning
                /*var file = BusinessAccessLayer.BusinessEntities.File.CreateNewFile<File>(Path.GetFileName(template.Name), String.Format("Template[{0}]", name));
                using (var stream = file.OpenWrite())
                {
                    using (var writter = new StreamWriter(stream))
                    {
                        writter.Write(templateText);
                    }
                }*/

                //Save to file
                System.IO.File.WriteAllText(template.Name, templateText);
            }

            if(Request.IsAjaxRequest())
                return new EmptyResult();
            
            return RedirectToAction("Index");
        }
    }
}
