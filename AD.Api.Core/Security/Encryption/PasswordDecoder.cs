using AD.Api.Core.Ldap.Passwords;
using System.Security;
using System.Text;
using Base64 = AD.Api.Extensions.Strings.Base64Extensions;

namespace AD.Api.Core.Security.Encryption;

internal sealed class PasswordDecoder : PasswordHandler
{
	public PasswordDecoder(PasswordOperationSettings settings) : base(settings)
	{
	}

	protected override SecureString Decrypt(ReadOnlySpan<char> password, Encoding encoding)
	{
		int length = Base64.GetByteLength(password);
		Span<byte> bytes = stackalloc byte[length];

		if (!Convert.TryFromBase64Chars(password, bytes, out int written))
		{
			throw new FormatException("Invalid base64 sequence.");
		}

		return ReadInPasswordBytes(bytes.Slice(0, written), encoding);
	}

	private static SecureString ReadInPasswordBytes(ReadOnlySpan<byte> passwordBytes, Encoding encoding)
	{
		int length = encoding.GetMaxCharCount(passwordBytes.Length);
		Span<char> chars = stackalloc char[length];
		int written = encoding.GetChars(passwordBytes, chars);

		SecureString ss = new();
		foreach (char c in chars.Slice(0, written))
		{
			ss.AppendChar(c);
		}

		return ss;
	}
}

