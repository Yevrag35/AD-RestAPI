using AD.Api.Attributes.Services;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel;

namespace AD.Api.Core.Ldap.Passwords;

public interface IPasswordService
{
	/// <summary>
	/// Indicates whether the service for the given functionality is enabled by configuration.
	/// </summary>
	bool IsFunctional { get; }
}
public interface IPasswordChangeService : IPasswordService
{
	IActionResult Change(in DomainQuery target, IPasswordRequest request);
}
public interface IPasswordResetService : IPasswordService
{
	IActionResult Reset(in DomainQuery target, IPasswordRequest request);
}

[DynamicDependencyRegistration]
internal sealed class PasswordService : IPasswordChangeService, IPasswordResetService
{
	private readonly PasswordHandler _decryptor;
	private readonly IRequestService _requests;

	public bool IsFunctional => true;

	public PasswordService(IRequestService requests, IOptions<PasswordOperationSettings> options)
	{
		_decryptor = PasswordHandler.CreateService(options.Value);
		_requests = requests;
	}

	public IActionResult Change(in DomainQuery target, IPasswordRequest request)
	{
		if (request.IsResetting())
		{
			return new ApiBadRequestResult("Password reset requests must use the reset endpoint.", ResultCode.UnwillingToPerform);
		}

		LdapConnection? connection = null;
		bool dontDispose = false;
		if (request.TryGetContinuation(out var continuation))
		{
			connection = continuation.ActiveConnection;
			dontDispose = true;
		}
		else if (_requests.Connections.GetConnection(in target, forceSsl: true).TryGetT1(out var error, out connection))
		{
			return error;
		}

		try
		{
			DistinguishedName dn = request.GetDistinguishedName();
			ModifyRequest modify = new((string)dn);

			_decryptor.EncodePasswordChange(request.OldPassword, request.NewPassword, modify);

			var oneOf = _requests.SendForResponse<ModifyResponse>(modify, connection);
			return oneOf.Match(
				f0: success => new AcceptedResult(),
				f1: fail => fail);
		}
		finally
		{
			if (!dontDispose)
			{
				connection.Dispose();
			}
		}
	}
	public IActionResult Reset(in DomainQuery target, IPasswordRequest request)
	{
		if (!request.IsResetting())
		{
			return new ApiBadRequestResult("Password change requests must use the change endpoint.", ResultCode.UnwillingToPerform);
		}

		LdapConnection? connection = null;
		bool dontDispose = false;
		if (request.TryGetContinuation(out var continuation))
		{
			connection = continuation.ActiveConnection;
			dontDispose = true;
		}
		else if (_requests.Connections.GetConnection(in target, forceSsl: true).TryGetT1(out var error, out connection))
		{
			return error;
		}

		try
		{
			DistinguishedName dn = request.GetDistinguishedName();
			ModifyRequest modify = new((string)dn);

			_decryptor.EncodePasswordReset(request.NewPassword, modify);

			var oneOf = _requests.SendForResponse<ModifyResponse>(modify, connection);
			return oneOf.Match(
				f0: success => new AcceptedResult(),
				f1: fail => fail);
		}
		finally
		{
			if (!dontDispose)
			{
				connection.Dispose();
			}
		}
	}

	private sealed class NoPasswordOperationService : IPasswordChangeService, IPasswordResetService
	{
		public bool IsFunctional => false;

		public IActionResult Change(in DomainQuery target, IPasswordRequest request)
		{
			return new ApiBadRequestResult("Password change operations are disabled.", ResultCode.UnwillingToPerform);
		}
		public IActionResult Reset(in DomainQuery target, IPasswordRequest request)
		{
			return new ApiBadRequestResult("Password reset operations are disabled.", ResultCode.UnwillingToPerform);
		}
	}

	[DynamicDependencyRegistrationMethod]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static void AddToServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection section = configuration
			.GetRequiredSection("Settings")
			.GetRequiredSection("PasswordOperations");

		services.AddOptions<PasswordOperationSettings>()
				.Bind(section)
				.ValidateOnStart()
				.ValidateDataAnnotations()
				.Validate(x =>
				{
					if (!x.Changes.Enabled && !x.Resets.Enabled)
					{
						return true;
					}

					return !x.Encryption.Required || !string.IsNullOrWhiteSpace(x.Encryption.SHA1Thumbprint)
					 || !string.IsNullOrWhiteSpace(x.Encryption.AESSharedKey);
				}, "Certificate encryption requires a SHA1 Thumbprint or an AES shared key to be set.");

		bool allEnabled = false;
		bool allDisabled = false;

		AddPasswordService<IPasswordChangeService>(
			services, section.GetRequiredSection("Changes"), ref allEnabled, ref allDisabled);
		AddPasswordService<IPasswordResetService>(
			services, section.GetRequiredSection("Resets"), ref allEnabled, ref allDisabled);

		if (allEnabled)
		{
			services.AddSingleton<PasswordService>();
		}
		else if (allDisabled)
		{
			services.AddSingleton<NoPasswordOperationService>();
		}
		else
		{
			services.AddSingleton<NoPasswordOperationService>()
					.AddSingleton<PasswordService>();
		}
	}

	private static void AddPasswordService<TService>(IServiceCollection services, IConfigurationSection section, ref bool allEnabled, ref bool allDisabled)
		where TService : class, IPasswordService
	{
		bool enabled = section.GetValue<bool>("Enabled");
		allEnabled |= enabled;
		allDisabled |= !enabled;

		ServiceDescriptor descriptor = enabled
			? new(typeof(TService), x => x.GetRequiredService<PasswordService>(), ServiceLifetime.Singleton)
			: new(typeof(TService), x => x.GetRequiredService<NoPasswordOperationService>(), ServiceLifetime.Singleton);

		services.Add(descriptor);
	}
}

