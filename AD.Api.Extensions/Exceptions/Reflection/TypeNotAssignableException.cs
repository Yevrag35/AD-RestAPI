namespace AD.Api.Reflection.Exceptions
{
    /// <summary>
    /// An exception thrown when one <see cref="Type"/> was attempted to be assigned to a variable of another, but the 
    /// type was not assignable.
    /// </summary>
    public class TypeNotAssignableException : AdApiReflectionException
    {
        /// <summary>
        /// The base type that <see cref="ImplementingType"/> should have inherited from or implemented.
        /// </summary>
        public Type BaseType { get; }
        /// <summary>
        /// The type that should have inherited from or implemented <see cref="BaseType"/>.
        /// </summary>
        public Type ImplementingType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="implementingType"></param>
        public TypeNotAssignableException(Type baseType, Type implementingType)
            : this(baseType, implementingType, (Exception?)null)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseType"></param>
        /// <param name="implementingType"></param>
        /// <param name="innerException"></param>
        public TypeNotAssignableException(Type baseType, Type implementingType, Exception? innerException)
            : this((string?)null, baseType, implementingType, innerException)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <param name="baseType"></param>
        /// <param name="implementingType"></param>
        /// <param name="innerException"></param>
        protected TypeNotAssignableException(string? message, Type baseType, Type implementingType, Exception? innerException)
            : base(message ?? string.Format(Errors.Exception_TypeNotAssignable, implementingType.GetName(), baseType.GetName()), innerException)
        {
            this.BaseType = baseType;
            this.ImplementingType = implementingType;
        }
    }
}

