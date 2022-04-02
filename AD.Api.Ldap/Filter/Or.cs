using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Filter
{
    public class Or : FilterKeyword, IFilterStatement
    {
        public List<IFilterStatement> Statements { get; } = new List<IFilterStatement>();

        public Or()
        {
        }

        public StringBuilder Generate(StringBuilder builder)
        {
            builder.Append("(|");

            this.Statements.ForEach(statement =>
            {
                statement.Generate(builder);

            });

            return builder.Append(CLOSE_PARA);
        }
    }
}
