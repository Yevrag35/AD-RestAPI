using Linq2Ldap;
using Linq2Ldap.Core.Attributes;
using Linq2Ldap.Core.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Components;
using AD.Api.Extensions;

namespace AD.Api.Services
{
    [SupportedOSPlatform("windows")]
    public abstract class ADServiceBase
    {
        protected SearchDomains Domains { get; }

        public ADServiceBase(SearchDomains domains)
        {
            this.Domains = domains;
        }


    }
}
