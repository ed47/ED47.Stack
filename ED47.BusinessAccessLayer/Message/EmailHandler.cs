using System;
using System.Net;
using System.Web;

namespace ED47.BusinessAccessLayer.Message
{
    public class EmailHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
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
            var id = context.Request["id"];

            if (id == null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.NoContent;
                return;
            }

            var email = EmailRepository.Get(Guid.Parse(id));

            if (email == null)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            context.Response.ContentType = "text/html";
            context.Response.Write(email.Body);
        }
    }
}
