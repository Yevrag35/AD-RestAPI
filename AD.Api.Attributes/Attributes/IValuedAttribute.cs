using System;

namespace AD.Api.Attributes
{
    /// <summary>
    /// An interface that defines a readable value that can be stored and is considered
    /// synonymous with the implementing <see cref="Attribute"/>.
    /// </summary>
    /// <typeparam name="TValue">The output type of the attribute's value.</typeparam>
    public interface IValuedAttribute<TValue> where TValue : notnull
    {
        /// <summary>
        /// The value that the <see cref="Attribute"/> is synonymous with.
        /// </summary>
        TValue Value { get; }
    }
}

