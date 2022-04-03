using MG.Attributes;
using System;

namespace AD.Api.Components
{
    public enum WellKnownObjects
    {
        [AdditionalValue("AA312825768811D1ADED00C04FD8D5CD")]
        Computers,

        [AdditionalValue("18E2EA80684F11D2B9AA00C04F79F805")]
        DeletedObjects,

        [AdditionalValue("A361B2FFFFD211D1AA4B00C04FD7D83A")]
        DomainControllers,

        [AdditionalValue("22B70C67D56E4EFB91E9300FCA3DC1AA")]
        ForeignSecurityPrincipals,

        [AdditionalValue("2FBAC1870ADE11D297C400C04FD8D5CD")]
        Infrastructure,

        [AdditionalValue("AB8153B7768811D1ADED00C04FD8D5CD")]
        LostAndFound,

        [AdditionalValue("6227F0AF1FC2410D8E3BB10615BB5B0F")]
        NTDSQuotas,

        [AdditionalValue("09460C08AE1E4A4EA0F64AEE7DAA1E5A")]
        ProgramData,

        [AdditionalValue("AB1D30F3768811D1ADED00C04FD8D5CD")]
        Systems,

        [AdditionalValue("A9D1CA15768811D1ADED00C04FD8D5CD")]
        Users,

        [AdditionalValue("1EB93889E40C45DF9F0C64D23BBB6237")]
        ManagedServiceAccounts
    }
}
