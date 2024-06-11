using System.ComponentModel.DataAnnotations;

namespace AD.Api.Core.Security.Encryption
{
    public interface IEncryptionResult
    {
        EncryptedCredential? Credential { get; }
        IReadOnlyList<ValidationResult> Errors { get; }
        [MemberNotNullWhen(true, nameof(Credential))]
        bool HasCredential { get; }
    }
    public static class EncryptionResult
    {
        private static readonly Dictionary<Type, object> _cache = new(2);

        public static EncryptionResult<T> Empty<T>() where T : EncryptedCredential
        {
            Type type = typeof(T);
            if (!_cache.TryGetValue(type, out object? val) || val is not EncryptionResult<T> result)
            {
                result = new() { Credential = null };
                _cache[type] = result;
            }

            return result;
        }
    }

    public sealed class EncryptionResult<T> : IEncryptionResult where T : EncryptedCredential
    {
        private readonly T? _credential;

        public required T? Credential
        {
            get => _credential;
            init
            {
                _credential = value;
                this.HasCredential = value is not null;
            }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        EncryptedCredential? IEncryptionResult.Credential => this.Credential;
        public IReadOnlyList<ValidationResult> Errors { get; init; } = [];
        [MemberNotNullWhen(true, nameof(Credential))]
        public bool HasCredential { get; private set; }
    }
}

