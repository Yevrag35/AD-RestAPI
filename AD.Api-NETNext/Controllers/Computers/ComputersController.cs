using AD.Api.Attributes;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Computers;

[ApiAuthorize]
[ApiController]
[Route(ROUTE_NAME)]
public sealed class ComputersController : ControllerBase
{
    private const string ROUTE_NAME = "computers";

    
}