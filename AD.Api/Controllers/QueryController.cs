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
    //[Route("[controller]")]
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private IMapper _mapper;
        private IADQueryService _userService;
        private INewNameService _nameService;

        public QueryController(IADQueryService userService, IMapper mapper)
        {
            _mapper = mapper;
            _userService = userService;
        }

        [HttpPost]
        [Route("user/[controller]")]
        public async Task<IActionResult> PostQueryAsync([FromQuery] string domain, [FromBody] UserQuery userQueryObj)
        {
            string exp = await userQueryObj.AsFilterAsync();
            List<JsonUser> results = await _userService.QueryAsync(domain, exp);
            if (null != results)
            {
                return Ok(results);
            }
            else
            {
                return BadRequest();
            }
        }
    }
}
