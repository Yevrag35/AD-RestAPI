using System;

namespace AD.Api.Ldap
{
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
        SCRIPT = 1,

        /// <summary>
        /// The user account is disabled. 
        ///</summary>
        ACCOUNTDISABLE = 2,

        /// <summary>
        /// The home directory is required. 
        ///</summary>
        HOMEDIR_REQUIRED = 8,

        /// <summary>
        /// The account is currently locked out. 
        ///</summary>
        LOCKOUT = 16,

        /// <summary>
        /// No password is required. 
        ///</summary>
        PASSWD_NOTREQD = 32,

        /// <summary>
        /// The user cannot change the password. 
        ///</summary>
        /// <remarks>
        /// Note:  You cannot assign the permission settings of PASSWD_CANT_CHANGE by directly modifying the UserAccountControl attribute. 
        /// For more information and a code example that shows how to prevent a user from changing the password, see User Cannot Change Password.
        /// </remarks>
        PASSWD_CANT_CHANGE = 64,

        /// <summary>
        /// The user can send an encrypted password. 
        ///</summary>
        ENCRYPTED_TEXT_PASSWORD_ALLOWED = 128,

        /// <summary>
        /// This is an account for users whose primary account is in another domain. This account provides user access to this domain, but not 
        /// to any domain that trusts this domain. Also known as a local user account. 
        ///</summary>
        TEMP_DUPLICATE_ACCOUNT = 256,

        /// <summary>
        /// This is a default account type that represents a typical user. 
        ///</summary>
        NORMAL_ACCOUNT = 512,

        /// <summary>
        /// This is a permit to trust account for a system domain that trusts other domains. 
        ///</summary>
        INTERDOMAIN_TRUST_ACCOUNT = 2048,

        /// <summary>
        /// This is a computer account for a computer that is a member of this domain. 
        ///</summary>
        WORKSTATION_TRUST_ACCOUNT = 4096,

        /// <summary>
        /// This is a computer account for a system backup domain controller that is a member of this domain. 
        ///</summary>
        SERVER_TRUST_ACCOUNT = 8192,

        Unused1 = 16384,

        Unused2 = 32768,

        /// <summary>
        /// The password for this account will never expire. 
        ///</summary>
        DONT_EXPIRE_PASSWD = 65536,

        /// <summary>
        /// This is an MNS logon account. 
        ///</summary>
        MNS_LOGON_ACCOUNT = 131072,

        /// <summary>
        /// The user must log on using a smart card. 
        ///</summary>
        SMARTCARD_REQUIRED = 262144,

        /// <summary>
        /// The service account (user or computer account), under which a service runs, is trusted for Kerberos delegation. Any such service 
        /// can impersonate a client requesting the service. 
        ///</summary>
        TRUSTED_FOR_DELEGATION = 524288,

        /// <summary>
        /// The security context of the user will not be delegated to a service even if the service account is set as trusted for Kerberos delegation. 
        ///</summary>
        NOT_DELEGATED = 1048576,

        /// <summary>
        /// Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys. 
        ///</summary>
        USE_DES_KEY_ONLY = 2097152,

        /// <summary>
        /// This account does not require Kerberos pre-authentication for logon. 
        ///</summary>
        DONT_REQUIRE_PREAUTH = 4194304,

        /// <summary>
        /// The user password has expired. This flag is created by the system using data from the Pwd-Last-Set attribute and the domain policy. 
        ///</summary>
        PASSWORD_EXPIRED = 8388608,

        /// <summary>
        /// The account is enabled for delegation. This is a security-sensitive setting; accounts with this option enabled should be strictly 
        /// controlled. This setting enables a service running under the account to assume a client identity and authenticate as that user to 
        /// other remote servers on the network.
        ///</summary>
        TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 16777216,

        PARTIAL_SECRETS_ACCOUNT = 67108864,

        USE_AES_KEYS = 134217728
    }
}
