namespace AD.Api.Core.Ldap.Filters
{
    /// <summary>
    /// An enumeration of possible keywords in an LDAP filter.
    /// </summary>
    [Flags]
    public enum FilterType
    {
        /// <summary>
        /// No filter type is specified.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// The given filter specifies that all sub-filters must be equal.
        /// </summary>
        And = 0x2,
        /// <summary>
        /// The given filter specifies that any sub-filters must be equal.
        /// </summary>
        Or = 0x4,
        /// <summary>
        /// The given filter negates the equality result of its sub-filter.
        /// </summary>
        Not = 0x8
    }
}