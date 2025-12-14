using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;

namespace AD.Api.Core.Authentication;

public sealed class NoNegotiateHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
	public NoNegotiateHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder)
		: base(options, loggerFactory, encoder)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		return Task.FromResult(AuthenticateResult.Fail("Negotiate is not enabled on this endpoint."));
	}
}
