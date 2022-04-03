using AutoMapper;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Runtime.Versioning;
using AD.Api.Models;
using AD.Api.Models.Entries;
using AD.Api.Attributes;
using AD.Api.Models.Collections;
using Linq2Ldap.Core.Types;

namespace AD.Api.Components
{
    [SupportedOSPlatform("windows")]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            this.CreateMap<User, UserQuery>();
            this.CreateMap<JsonUser, User>()
                .ForMember(nameof(User.ProxyAddresses), x => x.Ignore())
                .ForMember(nameof(User.EditOperations),
                (x) =>
                {
                    var resolver = new ValueCollectionResolver<ADSortedValueList<string>, string>();
                    x.MapFrom(resolver);
                });
            this.CreateMap<User, JsonUser>(MemberList.None)
                .ForMember(nameof(JsonUser.ProxyAddresses),
                    (x) =>
                    {
                        var resolver = new ValueCollectionResolver<ADSortedValueList<string>, string>(
                            x => x.ProxyAddresses, (vc, pm) => pm.NewValues = vc);

                        x.MapFrom(resolver);
                    });
            this.CreateMap<DirectoryEntry, User>(MemberList.None)
                .ForMember(nameof(User.EditOperations), x => x.Ignore())
                .ForAllOtherMembers(GetExpression<User>());

            this.CreateMap<DirectoryEntry, JsonUser>()
                .ForMember(nameof(JsonUser.ProxyAddresses),
                    x =>
                    {
                        string ldapAtt = AttributeReader.GetLdapValue(x.DestinationMember);
                        if (!string.IsNullOrWhiteSpace(ldapAtt))
                        {
                            var resolver = new DirEntryToJsonPAResolver();
                            x.MapFrom(resolver);
                        }
                    })
                .ForAllOtherMembers(GetExpression<JsonUser>());
        }

        private static Action<IMemberConfigurationExpression<DirectoryEntry, T, object>> GetExpression<T>()
        {
            return x =>
            {
                string ldapAtt = AttributeReader.GetLdapValue(x.DestinationMember);
                if (!string.IsNullOrEmpty(ldapAtt))
                {
                    x.MapFrom(col =>
                        col.Properties.Contains(ldapAtt)
                            ? col.Properties[ldapAtt].Value as string
                            : null
                    );
                }
                else
                {
                    x.Ignore();
                }
            };
        }

        private class ValueCollectionResolver<TCol, T> : IValueResolver<DirectoryEntry, JsonUser, object>,
            IValueResolver<User, JsonUser, object>, IValueResolver<JsonUser, User, object>
            where TCol : IValueCollection<T>, new()
        {
            private string _propName;
            private Action<IValueCollection<T>, PropertyMethod<T>> _action;
            private Func<User, IEnumerable<T>> _getValue;

            public ValueCollectionResolver()
            {
            }
            public ValueCollectionResolver(string propName)
            {
                _propName = propName;
            }
            public ValueCollectionResolver(Func<User, IEnumerable<T>> getValue, Action<IValueCollection<T>, PropertyMethod<T>> makeAction)
            {
                _action = makeAction;
                _getValue = getValue;
            }

            public object Resolve(DirectoryEntry source, JsonUser destination, object destMember, ResolutionContext context)
            {
                var col = new TCol();
                foreach (object s in source.Properties[_propName])
                {
                    col.Add((T)s);
                }

                return col;
            }

            public object Resolve(User source, JsonUser destination, object destMember, ResolutionContext context)
            {
                var col = new TCol();
                var pm = new PropertyMethod<T>();
                IEnumerable<T> values = _getValue(source);
                if (null == values)
                    return pm;

                foreach (T value in values)
                {
                    col.Add(value);
                }

                
                _action(col, pm);

                return pm;
            }

            public object Resolve(JsonUser source, User destination, object destMember, ResolutionContext context)
            {
                string ldapAtt = AttributeReader.GetJsonValue<User, LdapStringList>(x => x.ProxyAddresses, "proxyaddresses");
                var dict = new Dictionary<string, PropertyMethod<string>>(StringComparer.CurrentCultureIgnoreCase)
                {
                    {
                        ldapAtt, source.ProxyAddresses
                    }
                };

                return dict;
            }
        }

        private class DirEntryToJsonPAResolver : IValueResolver<DirectoryEntry, JsonUser, object>
        {
            private void AddValues(PropertyMethod<string> propertyMethod, IEnumerable<string> proxyAddresses)
            {
                propertyMethod.NewValues = new ProxyAddressCollection(proxyAddresses);
            }

            public object Resolve(DirectoryEntry source, JsonUser destination, object destMember, ResolutionContext context)
            {
                var pm = new PropertyMethod<string>();
                string ldapAtt = AttributeReader.GetJsonValue<User, LdapStringList>(x => x.ProxyAddresses, "proxyaddresses");
                IEnumerable<string> values = source.Properties[ldapAtt]?.Cast<string>();

                if (null == values)
                    return pm;

                this.AddValues(pm, values);

                return pm;
            }
        }
    }
}
