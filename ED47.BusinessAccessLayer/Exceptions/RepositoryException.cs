using System;
using System.Runtime.Serialization;

namespace ED47.BusinessAccessLayer
{
    /// <summary>
    /// Repository related exception.
    /// </summary>
    [Serializable]
    public sealed class RepositoryException : Exception
    {
        /// <summary>
        /// Creates a new RepositoryException instance.
        /// </summary>
        /// <param name="message">The exception message.</param>
        public RepositoryException(string message) : base(message)
        {
        }

        public RepositoryException() : this("Repository exception")
        {
            
        }

        public RepositoryException(String message, Exception innerException) : base(message,innerException)
        {
            
        }

         private RepositoryException(SerializationInfo info, StreamingContext context) : base(info,context)
         {
             
         }
    }
}