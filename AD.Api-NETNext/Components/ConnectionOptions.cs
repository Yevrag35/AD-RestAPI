using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace AD.Api.Services
{
    public interface IConnectionOptions
    {
        string? Domain { get; }
        bool DontDisposeHandle { get; }
        ClaimsPrincipal? Principal { get; }
        string? SearchBase { get; }
    }

    public record ConnectionOptions : IConnectionOptions
    {
        public string? Domain { get; set; }
        public bool DontDisposeHandle { get; set; } = true;
        public ClaimsPrincipal? Principal { get; set; }
        public string? SearchBase { get; set; }
    }
}
