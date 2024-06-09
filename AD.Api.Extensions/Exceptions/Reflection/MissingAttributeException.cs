namespace AD.Api.Reflection.Exceptions
{
    /// <summary>
    /// An exception thrown when an attribute is not found on a <see cref="Type"/> through reflection.
    /// </summary>
    public sealed class MissingAttributeException : AdApiReflectionException
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
            : base(string.Format(Errors.Exception_MissingAttr, attributeType.GetName()), (Exception?)null)
        {
            this.AttributeType = attributeType;
        }
    }
}

