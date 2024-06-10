using AD.Api.Core.Security;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Settings
{
    public sealed class RegisteredDomain
    {
        [Required]
        [MinLength(9)]
        public required string DefaultNamingContext { get; init; }
        public string[] DomainControllers { get; init; } = [];
        public string DomainName { get; set; } = string.Empty;
        public bool IsDefault { get; init; }
        [Required]
        public required bool IsForestRoot { get; init; }
        public string Name { get; set; } = string.Empty;
        public int? PortNumber { get; set; }
        public bool? UseSSL { get; init; }
    }
}

