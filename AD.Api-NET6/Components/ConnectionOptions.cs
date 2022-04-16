using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AD.Api.Services
{
    public record ConnectionOptions
    {
        public string? Domain { get; init; }
        public bool DontDisposeHandle { get; init; } = true;
        public ClaimsPrincipal? Principal { get; init; }
        public string? SearchBase { get; init; }
    }
}
