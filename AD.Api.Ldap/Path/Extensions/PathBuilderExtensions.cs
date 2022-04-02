using System;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Ldap.Path.Extensions
{
    public static class PathBuilderExtensions
    {
        public static IPathBuilder SetBase(this IPathBuilder builder, string baseDN)
        {
            builder.DistinguishedName = baseDN;
            return builder;
        }
        public static IPathBuilder UseSSL(this IPathBuilder builder, bool toggle = true)
        {
            builder.SSL = toggle;
            return builder;
        }
    }
}
