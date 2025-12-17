using System.Collections.Specialized;

namespace AD.Api.Collections.Intrinsics;

/// <summary>
/// Provides unsafe methods for manipulating the internal data storage of a <see cref="StringCollection"/>.
/// </summary>
public static class StringCollectionMarshal
{
	/// <summary>
	/// Replaces the internal data storage of the specified <see cref="StringCollection"/> with the provided <see cref="LdapPropertyList"/>.
	/// </summary>
	/// <param name="collection">The <see cref="StringCollection"/> whose internal data will be replaced.</param>
	/// <param name="replacement">The <see cref="LdapPropertyList"/> to use as the new internal data storage.</param>
	/// <returns>The modified <see cref="StringCollection"/> instance.</returns>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="collection"/> or <paramref name="replacement"/> is null.</exception>
	public static void ReplacePrivateField(StringCollection collection, ArrayList replacement)
	{
		GetDataList(collection) = replacement;
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "data")]
	private static extern ref ArrayList GetDataList(StringCollection collection);
}