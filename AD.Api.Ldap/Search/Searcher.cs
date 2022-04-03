using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Search
{
    public class Searcher
    {
        public Searcher()
        {
            var condition = new Or
            {
                LdapUser.GetFilter("mike.garvey@yevrag35.com", x => x.EmailAddress),
                new And
                {
                    LdapUser.GetFilter("Administrator", x => x.Name),
                    LdapUser.GetFilter(null, x => x.UserPrincipalName)
                }
            };

            StringBuilder builder = new(100);
            condition.WriteTo(builder);

            Console.WriteLine(builder.ToString());
        }

    }
}
