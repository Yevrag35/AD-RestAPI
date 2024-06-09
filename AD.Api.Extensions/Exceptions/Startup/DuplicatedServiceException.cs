using AD.Api.Reflection;

namespace AD.Api.Startup.Exceptions
{
    /// <summary>
    /// An exception that is thrown when a service is registered more than once in the Dependency Injection 
    /// container when it was not intended to be.
    /// </summary>
    public sealed class DuplicatedServiceException : AdApiStartupException
    {
        /// <summary>
        /// The service type that was duplicated.
        /// </summary>
        public Type DuplicateServiceType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicatedServiceException"/> class with the specified
        /// service type that was duplicated and the Dependency Injection type that caused the duplicate registration.
        /// </summary>
        /// <param name="serviceType">
        ///     <inheritdoc cref="DuplicateServiceType" path="/summary"/>
        /// </param>
        /// <param name="diType">
        ///     The Dependency Injection type that caused the duplicate registration.
        /// </param>
        public DuplicatedServiceException(Type serviceType, Type diType)
            : this(serviceType, diType, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DuplicatedServiceException"/> class with the specified
        /// service type that was duplicated, the Dependency Injection type that caused the duplicate registration, 
        /// and a optional reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="serviceType">
        ///     <inheritdoc cref="DuplicatedServiceException(Type, Type)" path="/param[1]"/>
        /// </param>
        /// <param name="diType">
        ///     <inheritdoc cref="DuplicatedServiceException(Type, Type)" path="/param[2]"/>
        /// </param>
        /// <param name="innerException">
        ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
        /// </param>
        public DuplicatedServiceException(Type serviceType, Type diType, Exception? innerException)
            : base(diType, GetMessage(serviceType), innerException)
        {
            this.DuplicateServiceType = serviceType;
        }

        private static string GetMessage(Type serviceType)
        {
            return string.Format(Errors.Exception_DuplicatedService, serviceType.GetName());
        }
    }
}

