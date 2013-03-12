using System;
using ED47.Communicator.Shared.BusinessEntities;

namespace ED47.Communicator.Admin.SampleModule
{
    /// <summary>
    /// A sample of a specific message.
    /// Its templates will be located under ED47.Stack.Sample/App_Data/Templates/Email/SampleMessage*.
    /// SampleMessageBody for the emails main body and SampleMessageSubject for the subject. Templates can either be TPL (*.html) or Razor (*.cshtml)
    /// </summary>
    public class SampleMessage : Message
    {
        /*TODO: Override the message business key generation if needed
        public override string GetBusinessKey()
        {
            return String.Format("Message[{0}]", SenderMemberId);
        }
        */

        /// <summary>
        /// Creates a new SampleMessage.
        /// </summary>
        /// <param name="recipientEmail">The recipient's email.</param>
        public static SampleMessage Create(string recipientEmail) //TODO: Add any message-specific parameters here
        {
            var invitation = new SampleMessage
            {
                //TODO: Initilialize any specific properties used by the template here
            };
            
            invitation.GroupLabel = invitation.GenerateGroupLabel();
            invitation.Init(recipientEmail, data: null); //TODO: Pass sender data via the "data" parameter
            invitation.Insert();
            
            return invitation;
        }

        /// <summary>
        /// The email grouping string generation method.
        /// </summary>
        private string GenerateGroupLabel()
        {
            //Emails can be grouped using a string. Modify this for different grouping string generation
            return String.Format("{0} / {1}", DateTime.Now.ToShortDateString(), typeof(SampleMessage));
        }
    }
}
