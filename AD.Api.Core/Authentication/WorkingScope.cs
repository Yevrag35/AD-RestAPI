using System.Runtime.InteropServices;

namespace AD.Api.Core.Authentication;

[StructLayout(LayoutKind.Auto)]
public readonly ref struct WorkingScope
{
	public readonly ReadOnlySpan<char> DistinguishedName;
	public readonly ReadOnlySpan<char> DomainName;
	public readonly AuthorizedRole RequiredRole;

	public WorkingScope(ReadOnlySpan<char> domain, ReadOnlySpan<char> distinguishedName, AuthorizedRole requiredRole)
	{
		DomainName = domain;
		DistinguishedName = distinguishedName;
		RequiredRole = requiredRole;
	}

	public static WorkingScope Open => new(string.Empty, string.Empty, AuthorizedRole.None);
}

