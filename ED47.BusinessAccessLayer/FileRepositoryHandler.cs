﻿using System;
using System.Web;
using System.Web.Security;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Handles the download of repository files.
    /// </summary>
    public class FileRepositoryHandler : IHttpHandler
    {

        public static Func<HttpContext, bool> IsAuthenticated;
        static FileRepositoryHandler()
        {
            IsAuthenticated = _isAuthenticated;
        }

        private static bool _isAuthenticated(HttpContext context)
        {
            return context.User.Identity.IsAuthenticated;
        }

        /// <summary>
        /// You will need to configure this handler in the web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        /// <summary>
        /// Gets the file from the repository.
        /// </summary>
        /// <param name="context">The HttpContext.</param>
        public void ProcessRequest(HttpContext context)
        {
            IFile file;
            var token = context.Request["token"];

            if (String.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = 401;
                context.Response.End();
                return;
            }

            int fileId;
            if (Int32.TryParse(context.Request["id"], out fileId))
            {
                file = FileRepositoryFactory.Default.Get(fileId);
            }
            else
            {
                var businessKey = context.Request["key"];
                file = FileRepositoryFactory.Default.GetFileByKey(businessKey);

            }

            if (file == null)
            {
                context.Response.StatusCode = 404;
                context.Response.End();
                return;
            }

            if (file.LoginRequired && !IsAuthenticated(context))
            {
                context.Response.StatusCode = 401;
                context.Response.End();
                return;
            }
           
            

            if (file.Guid.ToString().ToLowerInvariant() != token.Replace("{",string.Empty).Replace("}",string.Empty).ToLowerInvariant() )
            {
                context.Response.StatusCode = 401;
                context.Response.End();
                return;
            }

            context.Response.Clear();
            context.Response.ContentType = ED47.Stack.Web.MimeTypeHelper.GetMimeType(file.Name);
            if (!file.Encrypted)
            {
               
                using (var rs = file.OpenRead())
                {
                    if (rs == null)
                        return;

                    context.Response.AddHeader("Content-Disposition", String.Format("filename=\"{0}\";size={1};", file.Name, rs.Length));

                    if (rs.CanRead)
                        rs.CopyTo(context.Response.OutputStream);
                }
            }
            else
            {
                using (var rs = Cryptography.Decrypt(file.OpenRead(),file.KeyHash))
                {
                    if (rs == null)
                        return;

                    context.Response.AddHeader("Content-Disposition", String.Format("filename=\"{0}\";size={1};", file.Name, rs.Length));

                    if (rs.CanRead)
                        rs.CopyTo(context.Response.OutputStream);
                }
            }
           
            context.Response.OutputStream.Flush();
            context.Response.End();
        }

        #endregion
    }
}
