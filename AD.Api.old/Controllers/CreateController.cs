using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AD.Api.Models;
using AD.Api.Services;

namespace AD.Api.Controllers
{
    [SupportedOSPlatform("windows")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class CreateController : ControllerBase
    {
        private INewNameService _nameReader;
        private IADCreateService _createService;
        
        public CreateController(INewNameService nameService, IADCreateService createService)
            : base()
        {
            _nameReader = nameService;
            _createService = createService;
        }

        [HttpPost]
        [Route("user/[controller]")]
        public IActionResult CreateUser([FromBody] JsonCreateUser userRequest)
        {
            string constructedName = _nameReader.Construct(userRequest);
            //JsonUser user = _createService.CreateUser(userRequest, constructedName);
            return Ok(constructedName);
        }
    }
}
