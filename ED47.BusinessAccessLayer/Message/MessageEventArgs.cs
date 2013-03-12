using System;

namespace ED47.BusinessAccessLayer.Message
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(MessageFactory message)
        {
            this.Message = message;
        }

        public MessageFactory Message { get; set; }
    }
}