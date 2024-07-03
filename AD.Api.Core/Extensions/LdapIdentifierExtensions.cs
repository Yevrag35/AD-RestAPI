using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Extensions;

public static class LdapIdentifierExtensions
{
    public static bool IsSSL(this LdapDirectoryIdentifier identifier)
    {
        return identifier.PortNumber switch
        {
            636 => true,
            3269 => true,
            _ => false,
        };
    }

    public static LdapDirectoryIdentifier ToSSL(this LdapDirectoryIdentifier identifier)
    {
        return identifier.PortNumber switch
        {
            389 => new(identifier.Servers, 636, identifier.FullyQualifiedDnsHostName, identifier.Connectionless),
            3268 => new(identifier.Servers, 3269, identifier.FullyQualifiedDnsHostName, identifier.Connectionless),
            _ => identifier,
        };
    }
}