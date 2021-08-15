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
    public interface IADCreateService
    {

    }

    public class ADCreateService : IADCreateService
    {
        private IMapper _mapper;
        
        public ADCreateService(IMapper mapper)
        {
            _mapper = mapper;
        }

        //public JsonUser CreateUserAsync(JsonUser userDefinition)
        //{
        //    User user = _mapper.Map<User>(userDefinition);

        //    using (var dirEntry = user.GetDirectoryEntry())
        //    {

        //    }
        //}
    }
}
