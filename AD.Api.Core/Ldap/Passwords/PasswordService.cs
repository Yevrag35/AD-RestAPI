using AD.Api.Attributes.Services;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Passwords
{
    public interface IPasswordService
    {
    }
    public interface IPasswordChangeService : IPasswordService
    {
        IActionResult Change(in DomainQuery target, PasswordChangeRequest request);
    }
    public interface IPasswordResetService : IPasswordService
    {
        IActionResult Reset(in DomainQuery target, PasswordResetRequest request);
    }

    [DynamicDependencyRegistration]
    internal sealed class PasswordService : IPasswordChangeService, IPasswordResetService
    {
        private readonly PasswordHandler _decryptor;
        private readonly IRequestService _requests;

        public PasswordService(IRequestService requests, IOptions<PasswordOperationSettings> options)
        {
            _decryptor = PasswordHandler.CreateService(options.Value);
            _requests = requests;
        }

        public IActionResult Change(in DomainQuery target, PasswordChangeRequest request)
        {
            if (_requests.Connections.GetConnection(in target, forceSsl: true).TryGetT1(out var error, out LdapConnection? connection))
            {
                return error;
            }

            using (connection)
            {
                ModifyRequest modify = new(request.DistinguishedName);

                _decryptor.EncodePasswordChange(request.OldPassword, request.NewPassword, modify);

                var oneOf = _requests.SendForResponse<ModifyResponse>(modify, connection);
                return oneOf.Match(
                    f0: success => new AcceptedResult(),
                    f1: fail => fail);
            }
        }
        public IActionResult Reset(in DomainQuery target, PasswordResetRequest request)
        {
            if (_requests.Connections.GetConnection(in target, forceSsl: true).TryGetT1(out var error, out LdapConnection? connection))
            {
                return error;
            }

            using (connection)
            {
                ModifyRequest modify = new(request.DistinguishedName);

                _decryptor.EncodePasswordReset(request.NewPassword, modify);

                var oneOf = _requests.SendForResponse<ModifyResponse>(modify, connection);
                return oneOf.Match(
                    f0: success => new AcceptedResult(),
                    f1: fail => fail);
            }
        }

        private sealed class NoPasswordOperationService : IPasswordChangeService, IPasswordResetService
        {
            public IActionResult Change(in DomainQuery target, PasswordChangeRequest request)
            {
                return new ApiBadRequestResult("Password change operations are disabled.", ResultCode.UnwillingToPerform);
            }
            public IActionResult Reset(in DomainQuery target, PasswordResetRequest request)
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
}

