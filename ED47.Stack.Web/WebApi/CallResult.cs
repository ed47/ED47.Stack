using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ED47.Stack.Web
{
    /// <summary>
    /// Encapsulates the result of a call with the result data and validation messages among other things.
    /// </summary>
    public class CallResult
    {
        public CallResult() 
        {

            _messages = new List<string>();
            _scriptCommands = new List<string>();
            _validationItems = new List<ValidationItem>();
        }

        /// <summary>
        /// The result data.
        /// </summary>
        public dynamic ResultData { get; set; }

        private readonly List<ValidationItem> _validationItems;

        /// <summary>
        /// The call validation items.
        /// </summary>
        public ICollection<ValidationItem> ValidationItems
        {
            get { return _validationItems; }
           
        }

        private readonly List<string> _messages;

        /// <summary>
        /// The messages sent to the client.
        /// </summary>
        public ICollection<string> Messages
        {
            get { return _messages; }
          
        }

        private readonly List<string> _scriptCommands;

        /// <summary>
        /// The JavaScript commands to execute on the client.
        /// </summary>
        public ICollection<string> ScriptCommands
        {
            get { return _scriptCommands; }
           
        }
    }
}
