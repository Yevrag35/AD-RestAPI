using AD.Api.Core.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Web.Extensions;

public static class SidStringExtensions
{
	public static AcceptedResult ToAcceptedAt(this SidString sid, PathString pathBase, in DomainQuery target, object? value = null)
	{
		pathBase = pathBase.Add(sid.Value);
		string path = pathBase.Value!;
		QueryString query = target.ToQueryString();
		int length = path.Length + (query.HasValue ? query.Value!.Length : 0);

		string location = string.Create(length, (path, query), (chars, state) =>
		{
			state.path.CopyTo(chars);
			state.query.Value
				.AsSpan()
				.CopyTo(chars.Slice(state.path.Length));
		});

		return new AcceptedResult(location, value);
	}
}
