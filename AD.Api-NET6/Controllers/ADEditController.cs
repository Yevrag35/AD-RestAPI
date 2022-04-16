﻿using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;
using System.Net;
using System.Security.Principal;

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

        /// <summary>
        /// Edits an account.
        /// </summary>
        /// <param name="request">The request body containing the DistinguishedName (DN)
        /// of the object to modify with the specified edit operations.</param>
        /// <response code="202">The server successfully applied the modifications.</response>
        /// <response code="400">The JSON request body is malformed.</response>
        /// <response code="422">The encountered an error attempting to apply the modifications.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status202Accepted, Type = typeof(ISuccessResult))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type = typeof(IErroredResult))]
        public IActionResult PerformEdit([FromBody] EditOperationRequest request)
        {
            request.ClaimsPrincipal = this.HttpContext.User;

            ISuccessResult editResult = this.EditService.Edit(request);

            return editResult.Success
                ? Accepted(editResult)
                : UnprocessableEntity(editResult);
        }
    }
}
