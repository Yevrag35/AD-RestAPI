using System;

namespace AD.Api.Components.Exchange
{
    [Flags]
    public enum RemoteRecipientType
    {
        /// <summary>
        /// Indicates to the API that the attribute should be cleared for the user.
        /// </summary>
        /// <remarks>
        /// *NOTE - this value is not actually set on the user.  If this flag is set, all other flags are ignored.
        /// </remarks>
        Clear = -2147483648,
        /// <summary>
        /// Provisions a cloud mailbox for a user.
        /// </summary>
        ProvisionMailbox = 1,
        /// <summary>
        /// Provisions a cloud, in-place archive for an on-premises mailbox.
        /// </summary>
        ProvisionArchive = 2,
        /// <summary>
        /// Indicates a user's mailbox was migrated to the EOL using Exchange Hybrid.
        /// </summary>
        MigratedUserMailbox = 4,
        /// <summary>
        /// Deprovisions a cloud mailbox for a user.
        /// </summary>
        DeprovisionMailbox = 8,
        /// <summary>
        /// Deprovisions a user's cloud, in-place online archive.
        /// </summary>
        DeprovisionArchive = 16,
        /// <summary>
        /// Indicates that the user mailbox is a Room Mailbox.
        /// </summary>
        RoomMailbox = 32,
        /// <summary>
        /// Indicates the user mailbox is a Equipment Mailbox.
        /// </summary>
        EquipmentMailbox = 64,
        /// <summary>
        /// Indicates the user mailbox is a Shared Mailbox.
        /// </summary>
        SharedMailbox = 96
    }
}
