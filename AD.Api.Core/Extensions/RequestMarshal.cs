using AD.Api.Collections;
using AD.Api.Collections.Intrinsics;
using System.Collections.Specialized;

namespace AD.Api.Core.Extensions;

internal static class RequestMarshal
{
	internal static void ReplaceAttributes(SearchRequest request, LdapPropertyList list)
	{
		var collection = GetAttributesRef(request);
		StringCollectionMarshal.ReplacePrivateField(collection, list);
	}

	[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Attributes>k__BackingField")]
	private static extern ref StringCollection GetAttributesRef(SearchRequest request);
}
