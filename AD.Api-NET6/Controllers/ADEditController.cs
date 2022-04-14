﻿using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Net;

namespace AD.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("edit")]
    public class ADEditController : ControllerBase
    {
        private IEditService EditService { get; }

        public ADEditController(IEditService editService)
            : base()
        {
            this.EditService = editService;
        }

#if DEBUG
        [HttpPost]
        public IActionResult TestSendBack([FromBody] EditOperationRequest request)
        {
            return Ok(request);
        }
#endif

        [HttpPut]
        public IActionResult PerformEdit([FromBody] EditOperationRequest request)
        {
            OperationResult editResult = this.EditService.Edit(request);
            return editResult.Success
                ? Accepted(editResult)
                : BadRequest(editResult);
        }
    }
}
