using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filter
{
    public class Not : FilterKeyword, IFilterStatement
    {
        public PropertyEqual Statement { get; set; }

        public Not()
        {
        }
        public Not(string property, IConvertible value)
        {
            this.Statement = new PropertyEqual(property, value);
        }

        public StringBuilder Generate(StringBuilder builder)
        {
            builder.Append("(!");
            this.Statement.Generate(builder);

            return builder.Append(CLOSE_PARA);
        }
    }
}
