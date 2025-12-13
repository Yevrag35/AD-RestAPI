using System.Security.Claims;

namespace AD.Api.Security;

public static class ClaimsPrincipalExtensions
{

	/// <summary>
	/// Attempts to find the first claim of the specified type.
	/// </summary>
	/// <param name="principal"></param>
	/// <param name="type"></param>
	/// <param name="claim"></param>
	/// <returns>
	/// <see langword="true"/> if the claim was found; otherwise, <see langword="false"/>.
	/// </returns>
	public static bool TryFindFirst(this ClaimsPrincipal principal, string type, [NotNullWhen(true)] out Claim? claim)
	{
		ArgumentNullException.ThrowIfNull(principal);
		claim = principal.FindFirst(type);
		return claim is not null;
	}

	private const string SPLIT = ", ";
	/// <summary>
	/// Attempts to find the scopes from the specified claim.
	/// </summary>
	/// <param name="principal"></param>
	/// <param name="claimType"></param>
	/// <param name="scopes"></param>
	/// <returns></returns>
	public static bool TryGetScopesFromClaim(this ClaimsPrincipal principal, string claimType, [NotNullWhen(true)] out string[]? scopes)
	{
		if (!TryFindFirst(principal, claimType, out Claim? claim))
		{
			scopes = null;
			return false;
		}

		scopes = claim.Value.Split(SPLIT, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		return scopes.Length > 0;
	}
}