using AD.Api.Exceptions;
using System.ComponentModel;

namespace AD.Api.Collections.Exceptions
{
    /// <summary>
    /// An exception thrown when an attempt is made to modify a read-only collection.
    /// </summary>
    public class ReadOnlyException : AdApiException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyException"/> exception with the default message.
        /// </summary>
        public ReadOnlyException()
            : this(Errors.Exception_ReadOnlyCollection, (Exception?)null)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadOnlyException"/> exception with the specified message
        /// and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">
        ///     <inheritdoc cref="AdApiException(string)" path="/param"/>
        /// </param>
        /// <param name="innerException">
        ///     <inheritdoc cref="AdApiException(string, Exception)" path="/param[last()]"/>
        /// </param>
        public ReadOnlyException([Localizable(true)] string? message, Exception? innerException)
            : base(message, innerException)
        {
        }
    }
}

