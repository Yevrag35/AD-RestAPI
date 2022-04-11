using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Extensions
{
    public static class StringExtensions
    {
        public static string? Join(this IEnumerable<string>? strings, string joinBy = ", ")
        {
            if (strings is null)
                return null;

            return string.Join(joinBy, strings);
        }
    }
}
