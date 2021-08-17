using AutoMapper;
using Linq2Ldap;
using Linq2Ldap.Core;
using Linq2Ldap.Core.Models;
using Linq2Ldap.Core.Proxies;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Models;
using AD.Api.Models.Entries;

using Strings = AD.Api.Properties.Resource;

namespace AD.Api.Services
{
    public interface IADCreateService
    {

    }

    [SupportedOSPlatform("windows")]
    public class ADCreateService : IADCreateService
    {
        private IMapper _mapper;
        private SearchDomains _domains;

        public ADCreateService(IMapper mapper, SearchDomains searchDomains)
        {
            _domains = searchDomains;
            _mapper = mapper;
        }

        //public JsonUser CreateUser(JsonCreateUser userDefinition)
        //{
        //    DirectoryEntry dirEntry = null;
        //    if (userDefinition.UseDefaultOU())
        //    {
        //        SearchDomain defDom = _domains.GetDefaultDomain();
        //        dirEntry = userDefinition.GetDirectoryEntry(defDom.StaticDomainController, defDom.DistinguishedName);
        //    }
        //    else
        //    {
        //        dirEntry = userDefinition.GetDirectoryEntry(
        //            GetDomainControllerFromDefinition(userDefinition, _domains));
        //    }

        //    using (dirEntry)
        //    {

        //    }
        //}

        private static string GetDomainControllerFromDefinition(JsonCreateUser definition, SearchDomains domains)
        {
            string domainController = null;
            if (TryGetDNFromPath(definition.OUPath, out string domainDN) 
                && domains.TryGetValue(domainDN, out SearchDomain domain))
            {
                domainController = domain.StaticDomainController;
            }

            return domainController;
        }
        private static bool TryGetDNFromPath(string ouPath, out string domainDN)
        {
            Match match = Regex.Match(ouPath, Strings.DN_DomainCapture, RegexOptions.IgnoreCase);
            domainDN = match.Success && match.Groups.Count >= 2
                ? match.Groups[1].Value
                : null;

            return !string.IsNullOrWhiteSpace(domainDN);
        }
    }
}
