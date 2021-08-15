using AutoMapper;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Runtime.Versioning;
using AD.Api.Models;
using AD.Api.Models.Entries;
using AD.Api.Attributes;
using AD.Api.Models.Collections;

namespace AD.Api.Components
{
    [SupportedOSPlatform("windows")]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            this.CreateMap<User, UserQuery>();
            this.CreateMap<JsonUser, User>();
            this.CreateMap<User, JsonUser>()
                .ForMember(nameof(JsonUser.ProxyAddresses),
                    (x) =>
                    {
                        var resolver = new ValueCollectionResolver<ADSortedValueList<string>, string>(
                            x => x.ProxyAddresses, (vc, pm) => pm.NewValues = vc);

                        x.MapFrom(resolver);
                    });
            this.CreateMap<DirectoryEntry, User>(MemberList.None)
                .ForAllMembers(GetExpression<User>());

            this.CreateMap<DirectoryEntry, JsonUser>(MemberList.None)
                .ForMember(nameof(JsonUser.ProxyAddresses),
                    x =>
                    {
                        string ldapAtt = AttributeReader.GetLdapValue(x.DestinationMember);
                        if (!string.IsNullOrWhiteSpace(ldapAtt))
                        {
                            var resolver = new ValueCollectionResolver<ADSortedValueList<string>, string>(ldapAtt);
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
            IValueResolver<User, JsonUser, object>
            where TCol : IValueCollection<T>, new()
        {
            private string _propName;
            private Action<IValueCollection<T>, PropertyMethod<T>> _action;
            //private Func<JsonUser, PropertyMethod<T>> _getProp;
            private Func<User, IEnumerable<T>> _getValue;
            public ValueCollectionResolver(string propName)
            {
                _propName = propName;
            }
            public ValueCollectionResolver(Func<User, IEnumerable<T>> getValue, Action<IValueCollection<T>, PropertyMethod<T>> makeAction)
            {
                _action = makeAction;
                //_getProp = getProp;
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
                foreach (T value in _getValue(source))
                {
                    col.Add(value);
                }

                var pm = new PropertyMethod<T>();
                _action(col, pm);

                return pm;
            }
        }
    }
}
