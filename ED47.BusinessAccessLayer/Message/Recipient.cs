using ED47.Stack.Web;

namespace ED47.BusinessAccessLayer.Message
{
    public class Recipient
    {   
        internal JsonObject _Data;
        private readonly MessageFactory _Message;

        public Recipient(MessageFactory message)
        {
            _Message = message;
            if(message.JsonData != null)
                JsonData.Parent = Message.JsonData;
        }

        public bool Transmitted { get; set; }

        public string Address { get; set; }

        public MessageFactory Message
        {
            get { return _Message; }
        }

        public dynamic ModelData { get; set; }

        public JsonObject JsonData
        {
            get { return _Data ?? ( _Data = new JsonObject()); }
            set { _Data = value; }
        }
    }
}