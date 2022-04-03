using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Not : FilterContainer
    {
        public Not()
            : base(1)
        {
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(!");

            return base
                .WriteTo(builder)
                .Append((char)41);
        }
    }
}
