using AD.Api.Buffers;
using AD.Api.Core;

namespace AD.Api.Extensions;

public static class AcceptResultExtensions
{
	public static IActionResult WithLocation(
		this IActionResult result,
		string identifier,
		[ConstantExpected] string controllerRoute,
		in DomainQuery target)
	{
		if (result is not AcceptedResult accepted)
		{
			return result;
		}

		return WithAcceptedLocation(acceptedResult: accepted, identifier, controllerRoute, in target);
	}
	public static IActionResult WithLocation(
		this IActionResult result,
		string identifier,
		[ConstantExpected] string controllerRoute,
		[ConstantExpected] string actionRoute,
		in DomainQuery target)
	{
		if (result is not AcceptedResult accepted)
		{
			return result;
		}

		return WithAcceptedLocation(acceptedResult: accepted, identifier, controllerRoute, actionRoute, in target);
	}
	public static AcceptedResult WithAcceptedLocation(
		this AcceptedResult acceptedResult,
		string identifier,
		[ConstantExpected] string controllerRoute,
		in DomainQuery target)
	{
		using (SpanStringBuilder builder = new(stackalloc char[256]))
		{
			if (!controllerRoute.StartsWith('/'))
			{
				builder.Append('/');
			}

			builder.Append(controllerRoute);

			if (!controllerRoute.EndsWith('/'))
			{
				builder.Append('/');
			}

			builder.Append(identifier);
			builder.AppendIn(in target, target.UrlQueryLength);

			acceptedResult.Location = builder.ToString();
			return acceptedResult;
		}
	}
	public static AcceptedResult WithAcceptedLocation(
		this AcceptedResult acceptedResult,
		string identifier,
		[ConstantExpected] string controllerRoute,
		[ConstantExpected] string routeValues,
		in DomainQuery target)
	{
		using (SpanStringBuilder builder = new(stackalloc char[256]))
		{
			if (!controllerRoute.StartsWith('/'))
			{
				builder.Append('/');
			}

			builder.Append(controllerRoute);

			if (!controllerRoute.EndsWith('/'))
			{
				builder.Append('/');
			}

			builder.Append(identifier);

			if (!routeValues.StartsWith('/'))
			{
				builder.Append('/');
			}

			builder.Append(routeValues);
			builder.AppendIn(in target, target.UrlQueryLength);

			acceptedResult.Location = builder.ToString();
			return acceptedResult;
		}
	}
}
