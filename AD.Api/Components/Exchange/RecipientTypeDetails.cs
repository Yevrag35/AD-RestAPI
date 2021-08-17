using System;

namespace AD.Api.Components.Exchange
{
    [Flags]
    public enum RecipientTypeDetails : long
    {
        Clear = -2147483648L,
        UserMailbox = 1L,
        LinkedMailbox = 2L,
        SharedMailbox = 4L,
        RoomMailbox = 16L,
        EquipmentMailbox = 32L,
        MailUser = 128L,
        RemoteUserMailbox = 2147483648L,
        RemoteRoomMailbox = 8589934592L,
        RemoteEquipmentMailbox = 17179869184L,
        RemoteSharedMailbox = 34359738368
    }
}
