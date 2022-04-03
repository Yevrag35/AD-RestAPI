using System;
using System.Text;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Extensions
{
    public static class StringExtensions
    {
        public static string Format(this string str, params object[] arguments)
        {
            return string.Format(str, arguments);
        }
    }
}
