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
using Linq2Ldap.Core.Types;
using AD.Api.Models.Collections;

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

                    string proxAdd = AttributeReader.GetJsonValue<User, LdapStringList>(x => x.ProxyAddresses, "proxyaddresses");
                    if (user.EditOperations.ContainsKey(proxAdd))
                    {
                        ProcessProxyAddresses(proxAdd, dirEntry, user.EditOperations[proxAdd]);
                        user.EditOperations.Remove(proxAdd);
                    }
                    else if (user.EditOperations.Count > 0)
                        ProcessEditOperations(dirEntry, user.EditOperations);

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

        private static void ProcessProxyAddresses(string ldapAtt, DirectoryEntry dirEntry, PropertyMethod<string> propertyMethod)
        {
            PropertyValueCollection propValCol = dirEntry.Properties[ldapAtt];
            var newCol = new ProxyAddressCollection(propValCol.Cast<string>());
            newCol.ExceptWith(propertyMethod.OldValues);
            newCol.AddRange(propertyMethod.NewValues);
            propValCol.Clear();

            newCol.ForEach((address) =>
            {
                propValCol.Add(address);
            });
        }
        private static void ProcessEditOperations<T>(DirectoryEntry dirEntry, IDictionary<string, PropertyMethod<T>> operations)
        {
            foreach (var kvp in operations)
            {
                switch (kvp.Value.Operation)
                {
                    case Operation.Set:
                        dirEntry.Properties[kvp.Key].Clear();
                        goto case Operation.Add;

                    case Operation.Add:
                        foreach (T value in kvp.Value.NewValues)
                        {
                            dirEntry.Properties[kvp.Key].Add(value);
                        }

                        goto default;

                    case Operation.Remove:
                        foreach (T value in kvp.Value.OldValues)
                        {
                            dirEntry.Properties[kvp.Key].Remove(value);
                        }

                        break;

                    case Operation.Replace:
                        foreach (T value in kvp.Value.OldValues)
                        {
                            dirEntry.Properties[kvp.Key].Remove(value);
                        }

                        goto case Operation.Add;

                    default:
                        break;
                }
            }
        }
    }
}
