using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AD.Api.Exceptions;
using AD.Api.Ldap.Connection;
using AD.Api.Services;

namespace AD.Api.Controllers
{
    [Produces("application/json")]
    [Authorize]
    [ApiController]
    public class QueryController
    {
    }
}
