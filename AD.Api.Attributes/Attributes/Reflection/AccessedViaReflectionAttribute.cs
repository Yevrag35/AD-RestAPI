using System;

namespace AD.Api.Attributes.Reflection
{
    /// <summary>
    /// An attribute that indicates that the target class/struct member is accessed via reflection from other type(s).
    /// </summary>
    /// <remarks>
    /// Solely used to identify members that are accessed via reflection for visibility and maintainability purposes.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Event, AllowMultiple = false, Inherited = true)]
    public class AccessedViaReflectionAttribute : AdApiAttribute
    {
        /// <summary>
        /// The type or types that accesses the target member via reflection.
        /// </summary>
        public Type[] FromTypes { get; }

        /// <summary>
        /// Initializes the <see cref="AccessedViaReflectionAttribute"/> attribute class denoting the 
        /// type or types that accesses the target member via reflection.
        /// </summary>
        /// <param name="fromTypes">
        ///     <inheritdoc cref="FromTypes" path="/summary"/>
        /// </param>
        public AccessedViaReflectionAttribute(params Type[] fromTypes)
        {
            this.FromTypes = fromTypes ?? Type.EmptyTypes;
        }
    }
}

