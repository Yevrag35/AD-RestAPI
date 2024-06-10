using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Security.Encryption
{
    public sealed class EncryptionResult
    {
        public required EncryptedCredential Credential { get; init; }
        public IReadOnlyList<ValidationResult> Errors { get; init; } = [];
        public bool HasCredential => !this.Credential.IsEmpty;

        public static readonly EncryptionResult NoCredential = new()
        {
            Credential = EncryptedCredential.NoCredential,
        };
    }
}

