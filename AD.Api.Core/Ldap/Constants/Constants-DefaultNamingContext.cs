using AD.Api.Attributes;

namespace AD.Api.Core.Ldap
{
    [StaticConstantClass]
    public static class NamingContextConstants
    {
        public const string AUDITING_POLICY = "auditingPolicy";
        public const string CREATION_TIME = "creationTime";
        public const string DISTINGUISHED_NAME = AttributeConstants.DISTINGUISHED_NAME;
        public const string FSMO_ROLE_OWNER = "fSMORoleOwner";
        public const string GP_LINK = "gpLink";
        public const string MACHINE_ACCOUNT_QUOTA = "ms-DS-MachineAccountQuota";
        public const string MASTERED_BY = "masteredBy";
        public const string MAX_PWD_AGE = "maxPwdAge";
        public const string MIN_PWD_AGE = "minPwdAge";
        public const string MIN_PWD_LENGTH = "minPwdLength";
        public const string NETBIOS_NAME = "dc";
        public const string NEXT_RID = "nextRid";
        public const string NT_MIXED_DOMAIN = "nTMixedDomain";
        public const string OTHER_WELL_KNOWN_OBJECTS = "otherWellKnownObjects";
        public const string PWD_HISTORY_LENGTH = "pwdHistoryLength";
        public const string PWD_PROPERTIES = "pwdProperties";
        public const string SERVER_STATE = "serverState";
        public const string SUB_REFS = "subRefs";
        public const string SYSTEM_FLAGS = "systemFlags";
        public const string WELL_KNOWN_OBJECTS = "wellKnownObjects";
    }
}

