using System;

namespace ED47.BusinessAccessLayer.Message
{
    public class MessageActionEventArgs : EventArgs
    {
        public MessageActionEventArgs(MessageFactory message)
        {
            Message = message;
            CancelAction = false;
        }

        public MessageFactory Message { get; set; }
        public bool CancelAction { get; set; }
    }
}