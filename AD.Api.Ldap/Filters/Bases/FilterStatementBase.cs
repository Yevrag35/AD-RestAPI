using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filters
{
    public abstract record FilterStatementBase : IFilterStatement
    {
        public abstract FilterType Type { get; }

        public virtual bool Equals(IFilterStatement? other)
        {
            return this.Type == other?.Type;
        }
        public abstract StringBuilder WriteTo(StringBuilder builder);
    }
}
