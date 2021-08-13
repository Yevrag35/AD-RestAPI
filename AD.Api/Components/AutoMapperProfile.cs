using AutoMapper;
using System;
using System.DirectoryServices;
using System.Runtime.Versioning;
using AD.Api.Models;
using AD.Api.Models.Entries;
using AD.Api.Attributes;

namespace AD.Api.Components
{
    [SupportedOSPlatform("windows")]
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            this.CreateMap<User, UserQuery>();
            this.CreateMap<JsonUser, User>();
            this.CreateMap<User, JsonUser>();
            this.CreateMap<DirectoryEntry, User>(MemberList.None)
                .ForAllMembers(GetExpression<User>());

            this.CreateMap<DirectoryEntry, JsonUser>(MemberList.None)
                .ForAllMembers(GetExpression<JsonUser>());
        }

        private static Action<IMemberConfigurationExpression<DirectoryEntry, T, object>> GetExpression<T>()
        {
            return x =>
            {
                string ldapAtt = AttributeReader.GetLdapAttribute(x.DestinationMember);
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
    }
}
