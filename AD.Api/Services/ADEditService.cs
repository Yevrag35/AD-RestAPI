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
    public interface IADEditService
    {
        Task<JsonUser> EditUserAsync(JsonUser userToEdit);
    }

    [SupportedOSPlatform("windows")]
    public class ADEditService : IADEditService
    {
        private IMapper _mapper;

        public ADEditService(IMapper mapper)
        {
            _mapper = mapper;
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
    }
}
