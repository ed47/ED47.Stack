using System;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ED47.BusinessAccessLayer;
using ED47.BusinessAccessLayer.BusinessEntities;
using ED47.Stack.Web.Template;
using Newtonsoft.Json;

namespace ED47.BusinessAccessLayer.BusinessEntities
{
    public class BaseMessage : BusinessEntity
    {
        public virtual int Id { get; set; }

        public virtual DateTime CreationDate { get; set; }
        
        [MaxLength(60)]
        public virtual string MessageType { get; set; }

        [MaxLength(200)]
        public virtual string BusinessKey { get; set; }

        public virtual int? EmailId { get; set; }
       
        public virtual DateTime? OpenDate { get; set; }

        public virtual string GroupLabel { get; set; }
        
        [JsonIgnore]
        public Email Email
        {
            get
            {
                if(EmailId.HasValue && _email == null)
                {
                    _email = Email.Get(EmailId.Value);
                }
                
                return _email;
            }
            set { _email = value; }
        }

        public Template GetSubjectTpl()
        {
            return Subject != null ? new Template(Subject) : Template.Get("Email_" +  GetType().Name + "Subject");
        }

        public virtual Template GetBodyTpl()
        {
            return Body != null ? new Template(Body) : Template.Get("Email_" + GetType().Name + "Body");
        }

        private Email _email;
        
        public string Body { get; set; }
        public string Subject { get; set; }

        public object Data { get; set; }

        private string _cc;
        private string _bcc;
        private IEnumerable _fileBoxItemMessages;

        public string CC
        {
            get { return _cc; }
            set
            {
                _cc = value;

                if (Email == null) return;

                Email.CC = _cc;
            }
        }

        public string Bcc
        {
            get { return _bcc; }
            set
            {
                _bcc = value;

                if(Email == null) return;

                Email.Bcc = _bcc;
            }
        }
        
        public virtual void SetSubject(object data = null)
        {
            if (!String.IsNullOrWhiteSpace(Email.Subject))
                return;

            var tpl = GetSubjectTpl();
            Email.Subject = tpl != null ? tpl.Apply(data ?? Data ?? this) : (data!= null ? data.ToString() : "No subject");
            
        }

        public virtual void SetBody(object data = null)
        {
            if (!String.IsNullOrWhiteSpace(Email.Body))
                return;
            
            var tpl = GetBodyTpl();
            Email.Body = tpl != null ? tpl.Apply(data ?? Data ?? this) : (data != null ? data.ToString() : "No body");
           
        }

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        /// <param name="force">if set to <c>true</c> [force] the sending even if the message has been already sent..</param>
        public void Send(bool force = false)
        {
            SetBody();
            SetSubject();
            Email.Save();
            Email.Send(force);
        }
        
        public void Delete(bool deleteEmail = false)
        {
            if(deleteEmail)
                Email.Delete();

            BaseUserContext.Instance.Repository.Delete<Entities.BaseMessage, BaseMessage>(this);
        }
    }
}
