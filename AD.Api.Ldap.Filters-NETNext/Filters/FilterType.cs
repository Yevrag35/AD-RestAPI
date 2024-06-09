namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// An enumeration of possible keywords in an LDAP filter.
    /// </summary>
    [Flags]
    public enum FilterType
    {
        /// <summary>
        /// The given filter is used for equality.
        /// </summary>
        /// <remarks>
        ///     This is the only <see cref="FilterType"/> that is not accepted as a keyword.
        /// </remarks>
        Equal = 1,
        /// <summary>
        /// The given filter specifies that all sub-filters must be equal.
        /// </summary>
        And = 2,
        /// <summary>
        /// The given filter specifies that any sub-filters must be equal.
        /// </summary>
        Or = 4,
        /// <summary>
        /// The given filter negates the equality result of its sub-filter.
        /// </summary>
        Not = 8,
        /// <summary>
        /// The given filter specifies that all sub-filters must NOT be equal.
        /// </summary>
        Nor = 16,
        /// <summary>
        /// The given filter performs a bitwise AND operation for equality.
        /// </summary>
        Band = 32,
        /// <summary>
        /// The given filter performs a bitwise OR operation for equality.
        /// </summary>
        Bor = 64,
        /// <summary>
        /// The given filter uses the chain of interheritance to determine equality.
        /// </summary>
        Recurse = 128
    }
}
