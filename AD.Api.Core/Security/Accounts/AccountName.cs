using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;

namespace AD.Api.Core.Security.Accounts
{
    public abstract class AccountName : IAccountName
    {
        public static IAccountName Parse(ReadOnlySpan<char> userName)
        {
            char slash = '\\';
            if (userName.Contains(slash) && slash != userName[^1])
            {
                int index = userName.IndexOf(slash);
                ReadOnlySpan<char> domain = userName.Slice(0, index);
                ReadOnlySpan<char> name = userName.Slice(index + 1);
                return new DownLevelLogonName(domain.ToString(), name.ToString());
            }
            else if (userName.Contains('@'))
            {
                return new UserPrincipalName(userName.ToString());
            }
            else
            {
                return Empty;
            }
        }
        public static IAccountName Parse(ReadOnlySpan<byte> encodedBytes, Encoding encoding)
        {
            int length = encoding.GetCharCount(encodedBytes);
            Span<char> chars = stackalloc char[length];

            int written = encoding.GetChars(encodedBytes, chars);

            return Parse(chars.Slice(0, written));
        }

        public abstract void SetCredential(NetworkCredential credential);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!this.TryValidateName(validationContext, out ValidationResult? badResult))
            {
                yield return badResult;
            }
        }

        protected abstract bool TryValidateName(ValidationContext context, [NotNullWhen(false)] out ValidationResult? badResult);

        public static readonly IAccountName Empty = new EmptyName();

        private readonly struct EmptyName : IAccountName
        {
            public void SetCredential(NetworkCredential credential)
            {
                Debug.Fail("EmptyName should never be used to set credentials.");
            }

            public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            {
                return [];
            }
        }
    }
}

