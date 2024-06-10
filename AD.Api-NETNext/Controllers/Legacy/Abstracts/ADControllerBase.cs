﻿using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers
{
    public abstract class ADControllerBase : ControllerBase
    {
        protected IIdentityService Identity { get; }

        public ADControllerBase(IIdentityService identityService)
            : base()
        {
            this.Identity = identityService;
        }
    }
}