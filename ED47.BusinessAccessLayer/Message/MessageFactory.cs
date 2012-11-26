using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ED47.Stack.Web;

namespace ED47.BusinessAccessLayer.Message
{
    public class MessageFactory
    {
       

        public const string EmailRegex = @"^(\w+)([\-+.][\w]+)*@(\w[\-\w]*\.){1,5}([A-Za-z]){2,6}$";


        /// <summary>
        /// Determines whether [is email address] [the specified email].
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns>
        ///   <c>true</c> if [is email address] [the specified email]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsEmailAddress(string email)
        {

            return Regex.IsMatch(email, EmailRegex);
        }


        private readonly List<Attachment> _attachements = new List<Attachment>();
        private JsonObject _data = new JsonObject();
        private string _fromAddress;
        private readonly List<Recipient> _incorrectRecipient = new List<Recipient>();
        private readonly Dictionary<string, Recipient> _recipients = new Dictionary<string, Recipient>();
        private string _replyAddress;


        public MessageFactory()
        {
            IsHtml = true;
            FactoryAttachmentMode = AttachmentMode.AsLinks;
        }

        public MessageFactory(Template subjectTpl, Template bodyTpl) : this()
        {
            SubjectTpl = subjectTpl;
            BodyTpl = bodyTpl;
        }

        public MessageFactory(string recipient, Template subjectTpl, Template bodyTpl)
            : this(subjectTpl, bodyTpl)
        {
            AddRecipients(recipient);
        }

        public MessageFactory(string from, string recipient, Template subjectTpl, Template bodyTpl)
            : this(recipient, subjectTpl, bodyTpl)
        {
            if(!IsEmailAddress(from))
                throw new ArgumentException(String.Format("Invalid Address {0}", from));
            FromAddress = from;
        }

        public ICollection<Attachment> Attachments
        {
            get { return _attachements; }
        }

        public static Action<MessageFactory> DefaultSendAction { get; set; }
        public Action<MessageFactory> SendAction { get; set; }
       
        public static string DefaultReplyAddress { get; set; }
        public static string DefaultFromAddress { get; set; }


        public AttachmentMode FactoryAttachmentMode { get; set; }
        public bool IsHtml { get; set; }

        public string ReplyAddress
        {
            get { return _replyAddress ?? DefaultReplyAddress; }
            set
            {
                if (!IsEmailAddress(value))
                    throw new ArgumentException("Invalid Address");
                _replyAddress = value;
            }
        }

        public string FromAddress
        {
            get { return _fromAddress ?? DefaultFromAddress; }
            set
            {
                if (!IsEmailAddress(value))
                    throw new ArgumentException(String.Format("Invalid address : {0}", value));
                _fromAddress = value;
            }
        }

        public Template SubjectTpl { get; set; }
        public Template BodyTpl { get; set; }

        public dynamic ModelData
        {
            get; set; 
        }

        public JsonObject JsonData
        {
            get { return _data; }
            set { _data = value; }
        }

        public IEnumerable<Recipient> IncorrectRecipient
        {
            get { return _incorrectRecipient.AsEnumerable(); }
        }

        public IEnumerable<Recipient> Recipients
        {
            get { return _recipients.Values; }
        }

        /// <summary>
        /// The business key generator method.
        /// </summary>
        public Func<Recipient, string> BusinessKeyGenerator { get; set; }

        public void SetBody(string value)
        {
            var tpl = Template.Get(value);
            BodyTpl = tpl ?? new Template(value);
          
        }

        public string GetBody(Recipient recipient = null)
        {
            if (recipient != null && JsonData != null)
                JsonData["_recipient"] = recipient.JsonData;
            return BodyTpl == null ? "No subject" : BodyTpl.Apply(JsonData ?? ModelData);
        }

       

        public void SetSubject(string value)
        {
            var tpl = Template.Get(value);
            SubjectTpl = tpl ?? new Template(value);
            
        }

        public string GetSubject(Recipient recipient = null)
        {
            if(recipient != null && JsonData != null)
                JsonData["_recipient"] = recipient.JsonData;
            return SubjectTpl == null ? "No body" : SubjectTpl.Apply(JsonData ?? ModelData);
        }

     

        public static event EventHandler<MessageEventArgs> MessageSuccess;
        public static event EventHandler<MessageFailureEventArgs> MessageFailure;
        public static event EventHandler<MessageActionEventArgs> MessageBeforeSend;

        public event EventHandler<MessageEventArgs> Success;
        public event EventHandler<MessageFailureEventArgs> Failure;
        public event EventHandler<MessageActionEventArgs> BeforeSend;

        
       
        public void AddAttachment(string filename, bool requireLogin = false)
        {
            if(!File.Exists(filename)) return;

            var f = BusinessEntities.File.CreateNewFile<BusinessEntities.File>(Path.GetFileName(filename), "Attachment",
                                                                               0, requireLogin);

            f.Write(new FileInfo(filename));

            Attachments.Add(new Attachment(){File = f, SpecificVersion = true});
        }

        public void AddAttachment(BusinessEntities.File file, bool specificVersion = true)
        {
            Attachments.Add(new Attachment() { File = file, SpecificVersion = true });
        }

        public void AddAttachment(string businessKey)
        {
            var f = BusinessEntities.File.GetFileByKey<BusinessEntities.File>(businessKey);
            if(f != null)
                Attachments.Add(new Attachment() { File = f, SpecificVersion = true });
        }


        public void AddRecipient(string recipient, object data)
        {
            var Address = recipient.ToLowerInvariant().Trim();
            if (!IsEmailAddress(Address))
                return;
            if (!_recipients.ContainsKey(Address))
            {
                if(data is JsonObject)
                    _recipients.Add(Address, new Recipient(this) { Address = Address, JsonData = data as JsonObject });
                else
                {
                    JsonData = null;
                    _recipients.Add(Address, new Recipient(this) { Address = Address, ModelData = data });

                }
            }
        }

        public void AddRecipients(string recipients)
        {
            var all =
                recipients.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries).Select(
                    adr => adr.ToLowerInvariant().Trim());
            foreach (var a in all)
            {
                if (!IsEmailAddress(a))
                {
                    _incorrectRecipient.Add(new Recipient(this) {Address = a});
                    return;
                }

                if (!_recipients.ContainsKey(a))
                    _recipients.Add(a, new Recipient(this) {Address = a});
            }
        }

        public void AddRecipients(IEnumerable<string> recipients)
        {
            foreach (var a in recipients)
                AddRecipients(a);
        }

        public void AddRecipients(IEnumerable<string> recipients, IEnumerable<JsonObject> data)
        {
            var recs = recipients.ToArray();
            var data2 = data.ToArray();

            for (var i = 0; i < recs.Length; i++)
            {
                if (i < data2.Length)
                    AddRecipient(recs[i], data2[i]);
                else
                    AddRecipients(recs[i]);
            }
        }

        public bool HasRecipient(string recipient)
        {
            return _recipients.ContainsKey(recipient);
        }


        public void RemoveRecipients(string recipients)
        {
            var all =
                recipients.Split(new[] {',', ';'}, StringSplitOptions.RemoveEmptyEntries).Select(
                    adr => adr.ToLower().Trim());
            foreach (var a in all.Where(a => _recipients.ContainsKey(a)))
            {
                _recipients.Remove(a);
            }
        }

        public void RemoveRecipients(IEnumerable<string> recipients)
        {
            foreach (var a in recipients)
                RemoveRecipients(a);
        }


        public void Send()
        {
            try
            {
                if (_recipients.Count == 0)
                    throw new InvalidOperationException("The message must have at least one recipient");

                if (FromAddress == null)
                    throw new InvalidOperationException("The message must have a from address");


                if (BeforeSend != null)
                {
                    var e = new MessageActionEventArgs(this);
                    MessageBeforeSend(this, e);
                    if (e.CancelAction) return;
                }
                if (BeforeSend != null)
                {
                    var e = new MessageActionEventArgs(this);
                    BeforeSend(this, e);
                    if (e.CancelAction) return;
                }

                if (SendAction == null) 
                    DefaultSendAction(this); 
                else 
                    SendAction(this);
              
                if (MessageSuccess != null)
                    MessageSuccess(this, new MessageEventArgs(this));
                if (Success != null)
                    Success(this, new MessageEventArgs(this));
            }
            catch (Exception ex)
            {
                if (MessageFailure != null)
                    MessageFailure(this, new MessageFailureEventArgs(this, ex));
                if (Failure != null)
                    Failure(this, new MessageFailureEventArgs(this, ex));
            }
        }

       
    }
}