using AD.Api.Reflection;

namespace AD.Api.Exceptions.Reflection
{
    /// <summary>
    /// An exception thrown when an attribute is not found on a <see cref="Type"/> through reflection.
    /// </summary>
    public sealed class MissingAttributeException : AdApiException
    {
        /// <summary>
        /// The <see cref="Type"/> of the attribute that was searched for.
        /// </summary>
        public Type AttributeType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MissingAttributeException"/> exception with the 
        /// specified attribute type that was searched for.
        /// </summary>
        /// <param name="attributeType">
        ///     <inheritdoc cref="AttributeType" path="/summary"/>
        /// </param>
        public MissingAttributeException(Type attributeType)
            : base(string.Format(Errors.Exception_MissingAttr, attributeType.GetName()))
        {
            this.AttributeType = attributeType;
        }
    }
}

