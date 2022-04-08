using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Extensions
{
    public static class StringExtensions
    {
        public static string Combine(this string value, params string[] others)
        {
            if (others is null || others.Length <= 0)
                return value;

            int count = value.Length + others.Sum(x => x.Length);

            return string.Create(count, others, (chars, state) =>
            {

            });
        }
    }
}
