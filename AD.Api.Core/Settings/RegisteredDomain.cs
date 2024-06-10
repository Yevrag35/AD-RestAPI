using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Settings
{
    public sealed class RegisteredDomain
    {
        [Required]
        [MinLength(9)]
        public required string DefaultNamingContext { get; init; }
        public string[] DomainControllers { get; init; } = [];
        [Required]
        [MinLength(3)]
        public required string DomainName { get; init; }
        [Required]
        public required bool IsForestRoot { get; init; }
        public string Name { get; set; } = string.Empty;
        public bool UseSSL { get; init; }
    }
}

