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
    [Authorize]
    [ApiController]
    public class EditController : ControllerBase
    {
        private IMapper _mapper;
        private IADEditService _editService;

        public EditController(IADEditService editService, IMapper mapper)
            : base()
        {
            _editService = editService;
            _mapper = mapper;
        }

        [HttpPut]
        [Route("user/[controller]")]
        public async Task<IActionResult> EditUserAsync([FromBody] JsonUser editRequest)
        {
            var editedUser = await _editService.EditUserAsync(editRequest);
            if (null != editedUser)
            {
                var mapped = _mapper.Map<JsonUser>(editedUser);
                return Accepted(mapped);
            }
            else
            {
                return UnprocessableEntity();
            }
        }
    }
}
