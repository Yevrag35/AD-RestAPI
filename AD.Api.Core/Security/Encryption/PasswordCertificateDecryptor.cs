using AD.Api.Core.Ldap.Passwords;
using AD.Api.Exceptions;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Text;
using Base64 = AD.Api.Strings.Base64Extensions;

namespace AD.Api.Core.Security.Encryption;

internal sealed class PasswordCertificateDecryptor : PasswordHandler
{
	public PasswordCertificateDecryptor(PasswordOperationSettings options)
		: base(options)
	{

	}

	/// <exception cref="CryptographicException"/>
	/// <exception cref="FormatException"/>
	private static void DecodePassword(ReadOnlySpan<char> password, EnvelopedCms cms)
	{
		int length = Base64.GetByteLength(password);
		Span<byte> bytes = stackalloc byte[length];

		if (!Convert.TryFromBase64Chars(password, bytes, out int written))
		{
			throw new FormatException("Invalid base64 sequence.");
		}

		cms.Decode(bytes.Slice(0, written));
	}
	protected override SecureString Decrypt(ReadOnlySpan<char> password, Encoding encoding)
	{
		if (password.IsEmpty)
		{
			return new SecureString();
		}

		EnvelopedCms cms = new();
		try
		{
			DecodePassword(password, cms);
			cms.Decrypt();
		}
		catch (Exception e)
		{
			throw new AdApiException("Failed to decrypt the encrypted string", e);
		}

		SecureString secure = new();
		byte[] plainBytes = cms.ContentInfo.Content;
		int length = encoding.GetMaxCharCount(plainBytes.Length);

		Span<char> chars = stackalloc char[length];
		int written = encoding.GetChars(plainBytes, chars);

		foreach (char c in chars.Slice(0, written))
		{
			secure.AppendChar(c);
		}

		Array.Clear(plainBytes);
		return secure;
	}

	//private static X509Certificate2 GetCertificate(string? thumbprint)
	//{
	//    ArgumentException.ThrowIfNullOrWhiteSpace(thumbprint);

	//    if (TryGetCertificateFromStore(thumbprint, StoreLocation.LocalMachine, out X509Certificate2? userCert))
	//    {
	//        return userCert;
	//    }
	//    else if (TryGetCertificateFromStore(thumbprint, StoreLocation.LocalMachine, out X509Certificate2? compCert))
	//    {
	//        return compCert;
	//    }

	//    throw new AdApiStartupException(typeof(PasswordCertificateDecryptor),
	//        $"The certificate with thumbprint '{thumbprint}' was not found in either the CurrentUser or LocalMachine stores.");
	//}
	//private static bool TryGetCertificateFromStore(string thumbprint, StoreLocation location, [NotNullWhen(true)] out X509Certificate2? certificate)
	//{
	//    using X509Store store = new(location);
	//    store.Open(OpenFlags.ReadOnly);

	//    X509Certificate2Collection collection = store.Certificates
	//        .Find(X509FindType.FindByThumbprint, thumbprint, false);

	//    if (0 == collection.Count)
	//    {
	//        certificate = null;
	//        return false;
	//    }

	//    certificate = collection[0];
	//    return true;
	//}
}

