namespace AD.Api.Attributes.Services
{
    /// <summary>
    /// An attribute on decorated classes/structs that will automatically call a registration method with the 
    /// AD API application's dependency injection container at startup.
    /// </summary>
    /// <remarks>
    ///     Any class that uses this attribute to register itself must also have defined a <see langword="static"/>
    ///     method decorated with <see cref="DynamicDependencyRegistrationMethodAttribute"/> that will be called to execute 
    ///     the registration.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class DynamicDependencyRegistrationAttribute : AutomaticDependencyInjectionAttribute
    {
    }

    /// <summary>
    /// An attribute indicating that the decorated method is called exclusively to register dependencies with a
    /// AD API application's dependency injection container.
    /// </summary>
    /// <remarks>
    ///     For proper use of this attribute, the method must be <see langword="static"/> (with any visibility) and
    ///     with one of 2 overloads. The <see cref="IServiceCollection"/> parameter must always be the first, and 
    ///     optionally can contain <see cref="IConfiguration"/> as the second.
    ///     <para>
    ///         Examples of this are:
    ///     <code>
    ///         MethodName(IServiceCollection services)
    ///         // - or -
    ///         MethodName(IServiceCollection services, IConfiguration configuration)
    ///     </code>
    ///     </para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DynamicDependencyRegistrationMethodAttribute : AutomaticDependencyInjectionAttribute
    {
    }
}

