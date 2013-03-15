using System;
using ED47.BusinessAccessLayer.BusinessEntities;

namespace ED47.Communicator.Shared.BusinessEntities
{
    /// <summary>
    /// The business entity for this application's base message.
    /// </summary>
    public class Message : BaseMessage
    {
        //TODO: Copy your additional message fields here

        public virtual string GetBusinessKey()
        {
            //TODO: Change the base business key generation here. This will help indentify and retrieve related messages
            return String.Format("Message[{0}]", "*CHANGE ME*");
        }

        /// <summary>
        /// The message intialisation method.
        /// </summary>
        /// <param name="recipientEmail">The message recipient's email.</param>
        /// <param name="data">Data passed to the message template</param>
        public void Init(string recipientEmail, object data = null) //TODO: Add additional application-specific message parameters 
        {
            //TODO: Use application-specific message parameters here to initialize the message.

            if (String.IsNullOrWhiteSpace(MessageType))
                MessageType = GetType().Name;

            BusinessKey = GetBusinessKey();
            var email = new Email
            {
                Recipient = recipientEmail,
                FromAddress = "*CHANGE ME*", //TODO: Get the from address to one of the application-specific message parameters
                BusinessKey = GetBusinessKey(),
                IsHtml = true
            };

            if (data != null) Data = data;
            Email = email;
            SetBody();
            SetSubject(); 
            email.Insert();
            EmailId = email.Id;
        }

        public virtual void Insert()
        {
            UserContext.Instance.Repository.Add<Communicator.Entities.Message, Message>(this); //This must match the Message entity that inherits BaseMessage. By convention we simply call it "Message".
        }
    }
}
