﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class Email : BusinessEntity
    {
        public Email()
        {
            IsHtml = true;
        }

        public virtual int Id { get; set; }

        [MaxLength(250)]
        public virtual string BusinessKey { get; set; }

        [MaxLength(200)]
        public virtual string Recipient { get; set; }

        [MaxLength(200)]
        public virtual string FromAddress { get; set; }

        [MaxLength(500)]
        public virtual string Subject { get; set; }

        public virtual string Body { get; set; }

        public virtual DateTime?  TransmissionDate { get; set; }
        private List<EmailAttachment> _Attachments;
        public IEnumerable<EmailAttachment> Attachments
        {
            get
            {
                if (_Attachments == null)
                {
                    _Attachments = new List<EmailAttachment>();
                    _Attachments.AddRange(BaseUserContext.Instance.Repository.Where<Entities.EmailAttachment, EmailAttachment>(el => el.EmailId == Id));
                }
                return _Attachments;
            }
            set
            {
                if (value == null) return; 
                _Attachments = value.ToList();
            }
        }

        public string CC { get; set; }

        public void Send(bool force = false)
        {
            if(TransmissionDate.HasValue && ! force)
            {
                return;
            }

            var mailMessage = new MailMessage
            {
                Subject = Subject.Replace("\n", String.Empty).Replace("\r", String.Empty),
                Body = Body,
                IsBodyHtml = IsHtml,
            };
            
            var emailTestSettings = ConfigurationManager.AppSettings["TestEmailRecipients"];
            if (!String.IsNullOrWhiteSpace(emailTestSettings))
            {
                var testRecipients = emailTestSettings.Split(';');

                mailMessage.To.Clear();
                mailMessage.CC.Clear();
                mailMessage.Bcc.Clear();

                foreach (var testRecipient in testRecipients)
                {
                    mailMessage.To.Add(testRecipient);

                    if (!String.IsNullOrWhiteSpace(CC))
                        mailMessage.CC.Add(testRecipient);

                    if (!String.IsNullOrWhiteSpace(Bcc))
                        mailMessage.Bcc.Add(testRecipient);
                }
            }
            else
            {
                mailMessage.To.Add(Recipient);

                if (!String.IsNullOrWhiteSpace(CC))
                    mailMessage.CC.Add(CC);

                if (!String.IsNullOrWhiteSpace(Bcc))
                    mailMessage.Bcc.Add(Bcc);
            }

            var tmp = new List<Stream>();
            try
            {
                foreach (var attachment in Attachments)
                {
                    var s = attachment.File.OpenRead();
                    tmp.Add(s);
                    mailMessage.Attachments.Add(new Attachment(s,attachment.File.Name));
                }
                var smtpClient = new SmtpClient();
                smtpClient.Send(mailMessage);
                TransmissionDate = DateTime.Now;
                Save();
            }
            finally
            {
                foreach (var stream in tmp)
                {
                    stream.Dispose();
                }
            }
            
        }

        public string Bcc { get; set; }

        public bool IsHtml { get; set; }

        public virtual void Insert()
        {
            BaseUserContext.Instance.Repository.Add<Entities.Email, Email>(this);
        }

        public virtual void Save()
        {
            BaseUserContext.Instance.Repository.Update<Entities.Email, Email>(this);
        }

        public static Email Get(int id)
        {
            return BaseUserContext.Instance.Repository.Find<Entities.Email, Email>(el => el.Id == id);
        }

        public void Delete()
        {
            foreach (var emailAttachment in Attachments)
            {
                emailAttachment.Delete();
            }

            BaseUserContext.Instance.Repository.Delete<Entities.Email, Email>(this);
        }
    }
}
