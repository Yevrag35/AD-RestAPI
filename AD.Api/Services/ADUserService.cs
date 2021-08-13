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
    public interface IADUserService
    {
        Task<JsonUser> EditUserAsync(JsonUser userToEdit);
        Task<List<JsonUser>> QueryAsync(string domain, string ldapFilter);
    }

    [SupportedOSPlatform("windows")]
    public class ADUserService : ServiceBase<User>, IADUserService, IDisposable
    {
        private IMapper _mapper;

        public ADUserService(IMapper mapper, SearchDomains searchDomains)
            : base(searchDomains)
        {
            _mapper = mapper;
        }

        private JsonUser ConvertFromAD(User user)
        {
            return _mapper.Map<JsonUser>(user);
        }
        public Task<JsonUser> EditUserAsync(JsonUser editRequest)
        {
            return Task.Run(() =>
            {
                User user = _mapper.Map<User>(editRequest);
                using (var dirEntry = editRequest.GetDirectoryEntry())
                {
                    var properties = user.GetLdapProperties();

                    for (int i = 0; i < properties.Count; i++)
                    {
                        var item = properties[i];
                        var action = AttributeReader.GetAction(item.Item1);

                        action(dirEntry.Properties, (item.Item2, item.Item3));
                    }

                    dirEntry.CommitChanges();
                    dirEntry.RefreshCache();

                    return _mapper.Map<JsonUser>(dirEntry);
                }
            });
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
