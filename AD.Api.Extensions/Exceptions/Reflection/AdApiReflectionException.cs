namespace AD.Api.Exceptions.Reflection
{
    /// <summary>
    /// An <see langword="abstract"/> exception base class for exceptions thrown during operations involving reflection.
    /// </summary>
    public abstract class AdApiReflectionException : AdApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AdApiReflectionException"/> class with the specified message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        /// The message that describes the error. If no message is provided, a default message is used.
        /// </param>
        /// <param name="innerException">
        ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
        /// </param>
        protected AdApiReflectionException(string? message, Exception? innerException)
            : base(message ?? Errors.Exception_Reflection_Default, innerException)
        {
        }
    }
}

