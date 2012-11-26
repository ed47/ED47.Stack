namespace ED47.Stack.Web
{
    /// <summary>
    /// Represents a validation item.
    /// </summary>
    public class ValidationItem
    {
        /// <summary>
        /// The name of the validation.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the validated property.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The message of the validation.
        /// </summary>
        public string Message { get; set; }
    }
}