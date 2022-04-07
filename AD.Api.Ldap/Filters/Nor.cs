using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public sealed record Nor : FilterContainer
    {
        public sealed override FilterType Type => FilterType.Nor;

        public Nor()
            : base(2)
        {
        }

        public sealed override void Add(IFilterStatement? statement)
        {
            switch (statement?.Type)
            {
                case FilterType.Equal:
                    Equal equals = (Equal)statement;
                    base.Add(new Not(equals.Property, equals.Value));
                    break;

                case FilterType.Not:
                    base.Add(statement);
                    break;

                default:
                    break;
            }
        }

        public sealed override StringBuilder WriteTo(StringBuilder builder)
        {   
            builder.Append("(&");
            return base.WriteTo(builder).Append((char)41);
        }
    }
}
