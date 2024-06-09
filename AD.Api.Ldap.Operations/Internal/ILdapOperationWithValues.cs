namespace AD.Api.Ldap.Operations.Internal
{
    internal interface ILdapOperationWithValues : ILdapOperation
    {
        List<object> Values { get; }
    }
}
