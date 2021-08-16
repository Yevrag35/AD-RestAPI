using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AD.Api.Models.Converters
{
    public class CamelStringEnumConverter : StringEnumConverter
    {
        public CamelStringEnumConverter()
            : base(new CamelCaseNamingStrategy())
        {
        }
    }
}
