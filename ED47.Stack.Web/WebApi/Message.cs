namespace ED47.Stack.Web
{
    /// <summary>
    /// Represnets a message sent to the client from the server.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The message's text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The type of message.
        /// </summary>
        public string Type { get; set; }
    }
}
