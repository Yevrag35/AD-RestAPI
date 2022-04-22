using System;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    /// <summary>
    /// The type of modifying operation being performed to either attributes or objects themselves.
    /// </summary>
    [Flags]
    public enum OperationType
    {
        /// <summary>
        /// The default type and also used when the operation was performing
        /// <see cref="DirectoryEntry.CommitChanges"/>.
        /// </summary>
        Commit = -65536,

        /// <summary>
        /// Represents an operation where all existing values are overwritten.
        /// </summary>
        Set = 1,

        /// <summary>
        /// Represents an operation where one or more values are being removed.
        /// </summary>
        Remove = 2,

        /// <summary>
        /// Represents an operation where one or more values are being added.
        /// </summary>
        Add = 4,

        /// <summary>
        /// Represents an operation where one or more values are being replaced with new ones.
        /// </summary>
        Replace = 8,

        /// <summary>
        /// Represents an operation where all values are removed.
        /// </summary>
        Clear = 16,
        
        /// <summary>
        /// An operation that indicates a new object is being created.
        /// </summary>
        Create = 128,

        /// <summary>
        /// An operation that indicates an existing object is being moved to a new location.
        /// </summary>
        Move = 256,

        /// <summary>
        /// An operation that indicates an existing object is being deleted.
        /// </summary>
        Delete = 512,

        Rename = 1024
    }
}
