using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Net;

namespace AD.Api.Controllers
{
    public abstract class ADCreateController : ADControllerBase
    {
        protected ICreateService CreateService { get; }
        protected IResultService ResultService { get; }

        public ADCreateController(IIdentityService identityService, ICreateService createService, IResultService resultService)
            : base(identityService)
        {
            this.CreateService = createService;
            this.ResultService = resultService;
        }

        protected static CreateOperationRequest RemoveProperties(CreateOperationRequest request, params string[] propertiesToRemove)
        {
            if (propertiesToRemove is null || propertiesToRemove.Length <= 0)
                return request;

            Array.ForEach(propertiesToRemove, prop =>
            {
                _ = request.Properties.Remove(prop);
            });

            return request;
        }
    }
}
