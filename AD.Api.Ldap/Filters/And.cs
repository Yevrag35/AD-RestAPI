using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record And : FilterContainer
    {
        public And()
            : base(2)
        {
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {
            builder.Append("(&");

            return base
                .WriteTo(builder)
                .Append((char)41);
        }
    }
}
