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
using System.Threading.Tasks;
using AD.Api.Attributes;
using AD.Api.Components;
using AD.Api.Models;
using AD.Api.Models.Entries;

namespace AD.Api.Services
{
    public interface IADQueryService
    {
        Task<List<JsonUser>> QueryAsync(string domain, string ldapFilter);
    }

    [SupportedOSPlatform("windows")]
    public class ADQueryService : SearchServiceBase<User>, IADQueryService, IDisposable
    {
        private IMapper _mapper;

        public ADQueryService(IMapper mapper, SearchDomains searchDomains)
            : base(searchDomains)
        {
            _mapper = mapper;
        }

        private JsonUser ConvertFromAD(User user)
        {
            return _mapper.Map<JsonUser>(user);
        }
        public Task<List<JsonUser>> QueryAsync(string domain, string ldapFilter)
        {
            this.ValidateDomain(domain);
            this.Searcher.RawFilter = ldapFilter;
            return Task.Run(() =>
            {
                var list = this.Searcher.FindAll().ToList();
                return list.ConvertAll(new Converter<User, JsonUser>(ConvertFromAD));
            });
        }
    }
}
