using AD.Api.Core;
using MG.Extensions.Strings.Builders;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;

namespace AD.Api.Extensions
{
    public static class AcceptResultExtensions
    {
        public static IActionResult WithLocation(this IActionResult result, string identifier, [ConstantExpected] string controllerRoute, in DomainQuery target)
        {
            if (result is not AcceptedResult accepted)
            {
                return result;
            }

            return WithAcceptedLocation(acceptedResult: accepted, identifier, controllerRoute, in target);
        }
        public static IActionResult WithLocation(this IActionResult result, string identifier, [ConstantExpected] string controllerRoute, [ConstantExpected] string actionRoute, in DomainQuery target)
        {
            if (result is not AcceptedResult accepted)
            {
                return result;
            }

            return WithAcceptedLocation(acceptedResult: accepted, identifier, controllerRoute, actionRoute, in target);
        }
        public static AcceptedResult WithAcceptedLocation(this AcceptedResult acceptedResult, string identifier, [ConstantExpected] string controllerRoute, in DomainQuery target)
        {
            SpanStringBuilder builder = new(stackalloc char[256]);
            builder = controllerRoute.StartsWith('/')
                ? builder.Append(controllerRoute)
                : builder.Append('/').Append(controllerRoute);

            builder = controllerRoute.EndsWith('/')
                ? builder.Append(identifier)
                : builder.Append('/').Append(identifier);

            builder = builder.Append(target.UrlQueryLength, target, (chars, state) =>
            {
                state.AppendAsQuery(chars, out int written);
                return written;
            });

            acceptedResult.Location = builder.Build();

            return acceptedResult;
        }
        public static AcceptedResult WithAcceptedLocation(this AcceptedResult acceptedResult, string identifier, [ConstantExpected] string controllerRoute, [ConstantExpected] string routeValues, in DomainQuery target)
        {
            SpanStringBuilder builder = new(stackalloc char[256]);
            builder = controllerRoute.StartsWith('/')
                ? builder.Append(controllerRoute)
                : builder.Append('/').Append(controllerRoute);

            builder = controllerRoute.EndsWith('/')
                ? builder.Append(identifier)
                : builder.Append('/').Append(identifier);

            builder = routeValues.StartsWith('/')
                ? builder.Append(routeValues)
                : builder.Append('/').Append(routeValues);

            builder = builder.Append(target.UrlQueryLength, target, (chars, state) =>
            {
                state.AppendAsQuery(chars, out int written);
                return written;
            });

            acceptedResult.Location = builder.Build();

            return acceptedResult;
        }
    }
}
