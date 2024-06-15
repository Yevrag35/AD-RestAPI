using AD.Api.Statics;
using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Settings
{
    public sealed class EncryptionSettings : PathedSettings, IValidatableObject
    {
        public string CertificatePath { get; set; } = string.Empty;
        public string CertificateKeyPath { get; set; } = string.Empty;
        public string CertificatePassword { get; init; } = string.Empty;
        public required bool Required { get; init; }
        public string SHA1Thumbprint { get; init; } = string.Empty;

        public bool IsReadFromFile()
        {
            return !string.IsNullOrWhiteSpace(this.CertificatePath)
                && (!string.IsNullOrWhiteSpace(this.CertificatePassword)
                || !string.IsNullOrWhiteSpace(this.CertificateKeyPath));
        }

        private static bool IsPathValid(string? path)
        {
            return string.IsNullOrEmpty(path) || File.Exists(path);
        }
        public override void ResolvePaths(ReadOnlySpan<char> basePath)
        {
            if (basePath.IsEmpty)
            {
                basePath = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (!string.IsNullOrWhiteSpace(this.CertificatePath))
            {
                this.CertificatePath = PathConverter.ToAbsolutePath(this.CertificatePath, basePath);
            }

            if (!string.IsNullOrWhiteSpace(this.CertificateKeyPath))
            {
                this.CertificateKeyPath = PathConverter.ToAbsolutePath(this.CertificateKeyPath, basePath);
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!this.Required)
            {
                yield break;
            }


        }

        
    }
}

