using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Converters.Json
{
    public partial class FilterConverter
    {
        private NamingStrategy NamingStrategy { get; }

        public FilterConverter(NamingStrategy strategy)
        {
            this.NamingStrategy = strategy;
        }
    }
}
