using System;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Ldap.Extensions
{
    public static class ListExtensions
    {
        public static void ForEach<T>(this IList<T> list, Action<T> action)
        {
            for (int i = 0; i < list.Count; i++)
            {
                action(list[i]);
            }
        }
    }
}
