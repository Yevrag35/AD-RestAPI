using AD.Api.Ldap.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

using Strings = AD.Api.Ldap.Properties.Resources;

namespace AD.Api.Ldap.Path
{
    public struct WellKnownObject
    {
        private const char COMMA = (char)44;
        private const char RIGHT_ARROW = (char)62;

        private readonly string _path;
        private readonly WellKnownObjectValue _value;

        public string Path => _path;
        public WellKnownObjectValue Value => _value;

        private WellKnownObject(WellKnownObjectValue value, string path)
        {
            _value = value;
            _path = path;
        }

        public static WellKnownObject Create(WellKnownObjectValue value, string domainDn)
        {
            return new WellKnownObject(value, JoinCreate(GetGuidFromValue(value), domainDn));
        }

        public static implicit operator string(WellKnownObject wko)
        {
            return wko.Path;
        }

        private static string JoinCreate(string fromValue, string domainDn)
        {
            (string Value, string Format, string DN) tuple = (fromValue, Strings.LDAP_Format_WKO, domainDn);

            int totalLength = tuple.Value.Length + tuple.Format.Length + tuple.DN.Length + 2;

            return string.Create(totalLength, tuple, (chars, state) =>
            {
                int position = 0;
                state.Format.AsSpan().CopyTo(chars);

                position += tuple.Format.Length;

                state.Value.AsSpan().CopyTo(chars.Slice(position));
                position += state.Value.Length;

                chars[position++] = COMMA;

                state.DN.AsSpan().CopyTo(chars.Slice(position));
                position += state.DN.Length;

                chars[position] = RIGHT_ARROW;
            });
        }

        private static string GetGuidFromValue(WellKnownObjectValue value)
        {
            if (EnumReader<BackendValueAttribute>.TryGetAttribute(value, out BackendValueAttribute? attribute))
            {
                return attribute.Value;
            }
            else
                throw new InvalidOperationException("Something went horribly wrong...");
        }
    }
}
