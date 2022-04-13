using AD.Api.Ldap.Attributes;
using System;

namespace AD.Api.Ldap
{
    public enum CreationType
    {
        Generic,
        User,
        Group,
        Contact,
        Computer
    }

    [Flags]
    public enum GroupType : uint
    {
        BuiltIn = 1u,
        Global = 2u,
        DomainLocal = 4u,
        Universal = 8u,
        Security = 2147483648
    }

    public enum Protocol
    {
        Ldap,
        GlobalCatalog
    }

    [Flags]
    public enum UserAccountControl
    {
        /// <summary>
        /// The logon script is executed. 
        ///</summary>
        Script = 1,

        /// <summary>
        /// The user account is disabled. 
        ///</summary>
        Disabled = 2,

        /// <summary>
        /// The home directory is required. 
        ///</summary>
        HomeDirectoryIsRequired = 8,

        /// <summary>
        /// The account is currently locked out. 
        ///</summary>
        LockedOut = 16,

        /// <summary>
        /// The account is not subject to a possibly existing policy regarding the length of password. 
        /// </summary>
        /// <remarks>
        /// This means an account can have a shorter password than is required or it may even have no password at all, even if 
        /// empty passwords are not allowed. This property is not visible in the normal GUI tools.
        /// (e.g. - Active Directory Users and Copmputers)
        /// </remarks>
        PasswordNotRequired = 32,

        /// <summary>
        /// The account cannot change its password. 
        ///</summary>
        /// <remarks>
        /// You cannot assign the permission settings of <see cref="CannotChangePassword"/> by directly modifying the 
        /// <see cref="UserAccountControl"/> value.
        /// </remarks>
        CannotChangePassword = 64,

        /// <summary>
        /// The user can send an encrypted password. 
        /// </summary>
        /// <remarks>
        ///     The password for this account is stored encrypted in the directory but in a reversible form. As the term reversible 
        ///     already implies: In principle, you could also say that with this setting, the password of the account can be read with
        ///     the appropriate permissions.  You need the <see cref="CanSendEncryptedTextPassword"/> flag when an application needs 
        ///     to know the passwords of the accounts to authenticate them. This is for example the case when you want/have to use 
        ///     RAS (Remote Access) with the old CHAP Authentication, or if you want to use IIS Digest Authentication embedded in an 
        ///     Active Directory environment.  Normally, passwords are stored as irreversible hash values in the AD database. 
        ///     So you should NEVER use this option unless it is absolutely necessary.
        /// </remarks>
        CanSendEncryptedTextPassword = 128,

        /// <summary>
        /// Indicates the account is for users whose primary account is in another domain.
        ///</summary>
        ///<remarks>
        /// This account provides user access to this domain, but not 
        /// to any domain that trusts this domain.
        /// Also known as a local user account.
        /// </remarks>
        TempDuplicateAccount = 256,

        /// <summary>
        /// This is a default account type that represents a typical user. 
        ///</summary>
        NormalUser = 512,

        /// <summary>
        /// This is a permit to trust account for a system domain that trusts other domains. 
        ///</summary>
        InterDomainTrustAccount = 2048,

        /// <summary>
        /// This is a computer account for a computer that is a member of this domain. 
        ///</summary>
        WorkstationTrustAccount = 4096,

        /// <summary>
        /// This is a computer account for a system backup domain controller that is a member of this domain. 
        ///</summary>
        ServerTrustAccount = 8192,

        Unused1 = 16384,

        Unused2 = 32768,

        /// <summary>
        /// The password for this account will never expire. 
        ///</summary>
        ///<remarks>
        /// The account is not subject to an existing policy regarding a forced password change interval.
        /// </remarks>
        PasswordNeverExpires = 65536,

        /// <summary>
        /// Indicates that this is a Majority Node Set (MNS) account.
        ///</summary>
        /// <remarks>
        /// Such accounts are required for the operation of cluster nodes for Windows Server 2003 (and newer), in which the quorum data is not stored on a shared media drive. This flag should never be set for a user account. 
        /// </remarks>
        LogonAccountForMNS = 131072,

        /// <summary>
        /// The user must log on using a smart card. 
        ///</summary>
        SmartcardRequired = 262144,

        /// <summary>
        /// The service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. 
        /// </summary>
        /// <remarks>
        ///     Any such service can impersonate a client requesting the service.
        /// </remarks>
        TrustedForDelegation = 524288,

        /// <summary>
        /// The security context of the user will not be delegated to a service even if the service account is set as trusted 
        /// for Kerberos delegation. 
        ///</summary>
        NeverTrustedForDelegation = 1048576,

        /// <summary>
        /// Indicates that, in the Kerberos authentication of the account, ONLY the algorithm DES (Data Encryption Standard) may be used for the generation of tickets.
        /// </summary>
        /// <remarks>
        /// This should only be set for accounts which don't use a Windows machine to log on to the domain (Windows will always have at least DES and RC4 available).
        /// Since Vista and Windows Server 2008, there is the much more modern AES (Advanced Encryption Standard) algorithm for Kerberos authentication to a domain controller available. For signaling which algorithms are supported for authentication of a specific account, there is now the modern attribute msDS-SupportedEncryptionTypes available
        ///</remarks>
        UseDESKeyOnly = 2097152,

        /// <summary>
        /// This account does not require Kerberos pre-authentication for logon.
        /// </summary>
        /// <remarks>
        /// This is only for older Kerberos clients, which need the ability to login to the domain from foreign systems and which 
        /// does not support Kerberos pre-authentication. For accounts that log on from a Windows machine, or just for machine 
        /// accounts of Windows domain members, this flag flag should NEVER be set, for the pre-authentication prevents certain 
        /// types of dictionary attacks on the Kerberos login.
        /// </remarks>
        DONT_REQUIRE_PREAUTH = 4194304,

        /// <summary>
        /// The user password has expired. 
        /// </summary>
        /// <remarks>
        /// This flag is created by the system using data from the Pwd-Last-Set attribute and the 
        /// domain policy.  This is a constructed attribute so it cannot be used as a filter criterion in LDAP search operations.
        /// </remarks>
        PasswordExpired = 8388608,

        /// <summary>
        /// The account is enabled for delegation. 
        /// </summary>
        /// <remarks>
        /// This is a security-sensitive setting; accounts with this option enabled should 
        /// be strictly controlled. This setting enables a service running under the account to assume a client identity and 
        /// authenticate as that user to other remote servers on the network.
        /// </remarks>
        TrustedToAuthenticateForDelegation = 16777216,

        PartialSecretsAccount = 67108864,

        UseAESKeys = 134217728
    }

    public enum WellKnownObjectValue
    {
        [BackendValue("AA312825768811D1ADED00C04FD8D5CD")]
        Computers,

        [BackendValue("18E2EA80684F11D2B9AA00C04F79F805")]
        DeletedObjects,

        [BackendValue("A361B2FFFFD211D1AA4B00C04FD7D83A")]
        DomainControllers,

        [BackendValue("22B70C67D56E4EFB91E9300FCA3DC1AA")]
        ForeignSecurityPrincipals,

        [BackendValue("2FBAC1870ADE11D297C400C04FD8D5CD")]
        Infrastructure,

        [BackendValue("AB8153B7768811D1ADED00C04FD8D5CD")]
        LostAndFound,

        [BackendValue("6227F0AF1FC2410D8E3BB10615BB5B0F")]
        NTDSQuotas,

        [BackendValue("09460C08AE1E4A4EA0F64AEE7DAA1E5A")]
        ProgramData,

        [BackendValue("AB1D30F3768811D1ADED00C04FD8D5CD")]
        Systems,

        [BackendValue("A9D1CA15768811D1ADED00C04FD8D5CD")]
        Users,

        [BackendValue("1EB93889E40C45DF9F0C64D23BBB6237")]
        ManagedServiceAccounts
    }
}
