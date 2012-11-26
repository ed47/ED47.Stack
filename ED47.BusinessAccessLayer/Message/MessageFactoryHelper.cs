using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Linq;
using ED47.BusinessAccessLayer.BusinessEntities;
using File = ED47.BusinessAccessLayer.BusinessEntities.File;

namespace ED47.BusinessAccessLayer.Message
{
    /// <summary>
    /// Send a ED47.Message via SMTP
    /// </summary>
    public static class MessageFactoryHelper
    {

        public static void CreateFiles(MessageFactory message)
        {
            var outputPath = ConfigurationManager.AppSettings["ED47.BusinessAccessLayer.Message.FileMessage.OutputDirectory"];

            foreach (var r in Enumerable.Where<Recipient>(message.Recipients, r => !r.Transmitted))
            {
                string title = message.SubjectTpl.Apply(r._Data ?? r.ModelData);
                var subject = Regex.Replace(title, "[^\\w]", (match => ""));

                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                var content = string.Format("From : {0}", (object)message.FromAddress);
                content += string.Format("<br/>nTo : {0}", r.Address);
                content += string.Format("<br/>Date : {0}", DateTime.Now);
                content += string.Format("<br/>Subject : {0}", subject);
                content += string.Format("<br/>Message : <br/>{0}", message.BodyTpl.Apply(r._Data != null ? r.JsonData : r.ModelData));

                var messageGuid = Guid.NewGuid().ToString();
                message.FactoryAttachmentMode = AttachmentMode.AsLinks;
                message.IsHtml = true;
                foreach (var a in message.Attachments)
                {

                    var name = outputPath + "\\" + messageGuid + "_" + a.File.Name;
                    a.File.Write(new FileInfo(name));
                    content += string.Format("<br/>Attachment :<a href={0}>{1}</a>", name, a.File.Name);
                }

                System.IO.File.AppendAllText(outputPath + "\\" + r.Address + "_" + DateTime.Now.Ticks + ".html", content);
                r.Transmitted = true;
            }
        }

        /// <summary>
        /// Creates emails for each recipient from the message factory.
        /// </summary>
        /// <param name="message">The message factory to create emails from.</param>
        public static IEnumerable<Email> CreateEmails(MessageFactory message)
        {
            var emails = new List<Email>();

            foreach (var r in message.Recipients.Where(r => !r.Transmitted))
            {
                string businessKey = null;

                if (message.BusinessKeyGenerator != null)
                {
                    businessKey = message.BusinessKeyGenerator(r);
                }   

                var email = new Email()
                {
                    FromAddress = message.FromAddress,
                    Recipient = r.Address,
                    Subject = message.GetSubject(r),
                    Body = message.GetBody(r),
                    BusinessKey = businessKey
                };

                BaseUserContext.Instance.Repository.Add<Entities.Email, Email>(email);

                emails.Add(email);

                foreach (var attachment in message.Attachments)
                {
                    var ea = new EmailAttachment()
                    {
                        EmailId = email.Id,
                        FileId = attachment.File.Id
                    };
                    BaseUserContext.Instance.Repository.Add<Entities.EmailAttachment, EmailAttachment>(ea);
                }
                r.Transmitted = true;
            }

            return emails;
        }



        public static void SendSmtpMessage(MessageFactory message)
        {
            var smtp = new SmtpClient();

         
            foreach (var r in message.Recipients.Where(r => !r.Transmitted))
            {
                var m = new MailMessage(message.FromAddress, r.Address);
                
                var subject = message.GetSubject(r);

                subject = Regex.Replace(subject, "[\n\r\t]", (match => ""));

                m.Subject = subject;
                m.Body = message.GetBody(r);
                m.ReplyToList.Add(new MailAddress(message.ReplyAddress));
                m.IsBodyHtml = true;


                if (message.FactoryAttachmentMode == AttachmentMode.AsAttachments)
                {
                    foreach (var a in message.Attachments)
                    {
                        using (var stream = a.File.OpenRead())
                        {
                            var attachment = new System.Net.Mail.Attachment(stream, a.File.Name);
                            m.Attachments.Add(attachment);
                        }
                    }
                }
                if (message.FactoryAttachmentMode == AttachmentMode.AsLinks && message.Attachments.Count > 0)
                {
                    if (message.IsHtml)
                    {
                        message.SetBody(message.GetBody(r) + ("<div id='attachment' class='msg-attachments'><span>Attachments:</span></br><ul>" + String.Join("", message.Attachments.Select(
                            el =>
                            String.Format("<li class='msg-attachment'><a href='{0}'>{1}</a></li>", el.File.Url, el.File.Name))) + "</ul></div>"));
                    }
                    else
                    {
                        message.SetBody(message.GetBody(r) + ("\r\nAttachments:\r\n" + String.Join("\n\r", message.Attachments.Select(
                            el =>
                            String.Format("{1} : {0}", el.File.Url, el.File.Name)))));
                    }
                }
                smtp.Send(m);
                r.Transmitted = true;
            }
        }
    }
}