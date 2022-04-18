using AD.Api.Ldap.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AD.Api.Components.Exchange
{
    [LdapEnum("msExchRecipientDisplayType")]
    public enum RecipientDisplayType
    {
        Clear = -2147483648,
        MailUserRemoteUserMailbox = -2147483642,
        MailUserRemoteRoomMailbox = -2147481850,
        MailUserRemoteEquipmentMailbox = -2147481594,
        SharedUserMailbox = 0,
        MailUniversalDistributionGroup = 1,
        MailContact = 6,
        RoomUserMailbox = 7,
        EquipmentUserMailbox = 8,
        UserMailbox = 1073741824,
        MailUniversalSecurityGroup = 1073741833
    }
}
