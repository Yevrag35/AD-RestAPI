using AD.Api.Exceptions;
using AD.Api.Reflection;
using System.ComponentModel;

namespace AD.Api.Startup.Exceptions
{
    /// <summary>
    /// Represents an error thrown during a Dependency Injection (DI) operation during the startup of an 
    /// AD Api application or related library.
    /// </summary>
    public class AdApiStartupException : AdApiException
    {
        /// <summary>
        /// The class type of the Dependency Injection extension that caused
        /// the exception.
        /// </summary>
        public Type DIType { get; }

        /// <summary>
        /// <inheritdoc cref="AdApiStartupException(Type)" path="/protected"/>.
        /// </summary>
        /// <protected>
        ///     <summary>
        ///     Initializes a new instance of the <see cref="AdApiStartupException"/> class identifying the specified
        ///     Depedency Injection class as the source of the exception</summary>
        /// </protected>
        /// <param name="diType">The type of the static class that caused the exception.</param>
        public AdApiStartupException(Type diType)
            : this(diType, null, null)
        {
        }

        /// <inheritdoc cref="AdApiStartupException(Type)" path="/*[not(self::summary) and not(self::protected)]"/>
        /// <summary>
        /// <inheritdoc cref="AdApiStartupException(Type)" path="/protected"/>
        /// <inheritdoc cref="AdApiStartupException(Type, Exception)" path="/protected"/>
        /// </summary>
        /// <protected>
        ///     <summary>and a reference to the inner exception that is the cause.</summary>
        /// </protected>
        /// <param name="innerException">
        ///     <inheritdoc cref="Exception(string, Exception)" path="/param[last()]"/>
        /// </param>
        /// <param name="diType"><inheritdoc cref="AdApiStartupException(Type)"/></param>
        public AdApiStartupException(Type diType, Exception? innerException)
            : this(diType, null, innerException)
        {
        }

        /// <inheritdoc cref="AdApiStartupException(Type, Exception)"
        ///     path="/*[not(self::summary) and not(self::protected)]"/>
        /// <summary>
        /// <inheritdoc cref="AdApiStartupException(Type)" path="/protected"/> with a specified error message.
        /// </summary>
        /// <param name="diType"><inheritdoc cref="AdApiStartupException(Type)"/></param>
        /// <param name="message"><inheritdoc cref="Exception(string)" path="/param"/></param>
        /// <param name="innerException"><inheritdoc cref="AdApiStartupException(Type, Exception)"/></param>
        public AdApiStartupException(Type diType, [Localizable(true)] string? message)
            : base(GetBaseMessageFromType(message, diType), null)
        {
            this.DIType = diType;
        }

        /// <inheritdoc cref="AdApiStartupException(Type, Exception)"
        ///     path="/*[not(self::summary) and not(self::protected)]"/>
        /// <summary>
        /// <inheritdoc cref="AdApiStartupException(Type)" path="/protected"/> with a specified error message
        /// <inheritdoc cref="AdApiStartupException(Type, Exception)" path="/protected"/>
        /// </summary>
        /// <param name="diType"><inheritdoc cref="AdApiStartupException(Type)"/></param>
        /// <param name="message"><inheritdoc cref="Exception(string)" path="/param"/></param>
        /// <param name="innerException"><inheritdoc cref="AdApiStartupException(Type, Exception)"/></param>
        public AdApiStartupException(Type diType, [Localizable(true)] string? message, Exception? innerException)
            : base(GetBaseMessageFromType(message, diType), innerException)
        {
            this.DIType = diType;
        }

        private static string GetBaseMessageFromType(string? message, Type type)
        {
            return GetMessageOrUseDefault(message, Errors.Exception_DI, type.GetName());
        }
    }
}

