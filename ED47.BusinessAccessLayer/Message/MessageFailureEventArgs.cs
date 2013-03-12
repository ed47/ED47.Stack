using System;
using System.Collections;

namespace ED47.BusinessAccessLayer.Message
{
    public class MessageFailureEventArgs : MessageEventArgs
    {
        public MessageFailureEventArgs(MessageFactory message, Exception exception)
            : base(message)
        {
            Exception = exception;
        }

        public Exception Exception { get; set; }
    }
}