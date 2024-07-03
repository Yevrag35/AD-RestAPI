using AD.Api.Strings.Spans;
using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace AD.Api.Validation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class SHA1ThumbprintAttribute : ValidatablePropertyAttribute<string>
    {
        private const int THUMBPRINT_LENGTH = 40;
        private static readonly SearchValues<char> _sha1Chars;
        static SHA1ThumbprintAttribute()
        {
            CharRange numerals = new('0', '9');
            CharRange lowercase = new('a', 'f');
            CharRange uppercase = new('A', 'F');

            Span<char> chars = stackalloc char[numerals.Length + lowercase.Length + uppercase.Length];
            numerals.CopyTo(chars);
            int pos = numerals.Length;

            lowercase.CopyTo(chars.Slice(pos));
            pos += lowercase.Length;

            uppercase.CopyTo(chars.Slice(pos));

            _sha1Chars = SearchValues.Create(chars);
        }

        public SHA1ThumbprintAttribute()
        {
        }

        public override bool RequiresValidationContext => false;
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null)
            {
                return ValidationResult.Success;
            }
            else if (value is IValidatableThumbprint validatable)
            {
                return ValidateNestedProperty(validatable, validationContext);
            }

            string[]? memberNames = null;

            if (value is not string sha1Str)
            {
                memberNames ??= [validationContext.MemberName ?? string.Empty];
                return new ValidationResult("The value is not a string.", memberNames);
            }
            else if (sha1Str.Length != THUMBPRINT_LENGTH)
            {
                memberNames ??= [validationContext.MemberName ?? string.Empty];
                return new ValidationResult("The value is not of the correct length - Thumbprints must be 40 characters in length.", memberNames);
            }
            else if (sha1Str.AsSpan().ContainsAnyExcept(_sha1Chars))
            {
                memberNames ??= [validationContext.MemberName ?? string.Empty];
                return new ValidationResult("The SHA1 Thumbprint contains invalid characters. Only 'A-F', 'a-f', and '0-9' are allowed.", memberNames);
            }

            return ValidationResult.Success;
        }
    }
}

