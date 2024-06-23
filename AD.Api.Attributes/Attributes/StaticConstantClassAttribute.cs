using System;

namespace AD.Api.Attributes
{
    /// <summary>
    /// Indicates that the decorated class is a <see langword="static"/> class whose members are 
    /// <see langword="const"/> values.
    /// </summary>
    /// <remarks>
    /// Any class marked with this attribute should be <see langword="static"/> and contain only 
    /// <see langword="const"/> values and will be excluded from Dependency Injection startup scanning.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class StaticConstantClassAttribute : AdApiAttribute
    {
    }
}

