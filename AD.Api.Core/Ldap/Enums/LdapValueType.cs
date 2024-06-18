namespace AD.Api.Core.Ldap
{
    public enum LdapValueType
    {
        Object = 0,
        String,
        Integer,
        Long,
        Guid,
        Boolean,
        DateTime,
        ByteArray,

        // Array types
        StringArray = 1000,
        IntegerArray,
        LongArray,
        GuidArray,
        BooleanArray,
        DateTimeArray,
        ByteTwoRankArray,
        ObjectArray,
    }
}

