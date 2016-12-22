using System.Linq;
using System.Web;
using Ionic.Zip;

namespace ED47.Stack.Web
{
    /// <summary>
    /// Serves all the XML translation files and templates in a Zip file.
    /// </summary>
    public class AppDataDownloadHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            using(var zip = new ZipFile())
            {
                zip.AddDirectory(HttpContext.Current.Server.MapPath(Multilingual.Multilingual.GetResourceFilesPath()), "Translations");
                zip.AddFiles(Template.Template.Templates.Select(el => el.Value), true, "Templates");
                zip.Save(context.Response.OutputStream);
            }
            context.Response.ContentType = "application/zip";
            context.Response.AddHeader("content-disposition", "attachment; filename=AppData.zip");
            context.Response.Flush();
        }
    }
}
