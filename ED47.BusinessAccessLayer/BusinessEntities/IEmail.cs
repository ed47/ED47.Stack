using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Net.Mail;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public interface IEmail
    {
        [MaxLength(250)]
        string BusinessKey { get; set; }

        [MaxLength(200)]
        string Recipient { get; set; }

        [MaxLength(200)]
        string FromAddress { get; set; }

        [MaxLength(500)]
        string Subject { get; set; }

        string Body { get; set; }
        DateTime? TransmissionDate { get; set; }
        IEnumerable<EmailAttachment> Attachments { get; set; }
        string CC { get; set; }
        string Bcc { get; set; }
        bool IsHtml { get; set; }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="force">if set to <c>true</c> force sending the message.</param>
        /// <param name="from">The optional from address.</param>
        void Send(bool force = false, string from = null);

        IEnumerable<Stream> AddAttachments(MailMessage mailMessage);
        void AddRecipients(MailMessage mailMessage);
    }
}