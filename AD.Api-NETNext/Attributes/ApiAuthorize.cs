using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.Authorization;

namespace AD.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class ApiAuthorizeAttribute : Attribute, IAuthorizeData
{
	private static readonly string _schemes =
		string.Join(",", JwtBearerDefaults.AuthenticationScheme, NegotiateDefaults.AuthenticationScheme);

	public string AuthenticationSchemes => _schemes;
	string? IAuthorizeData.AuthenticationSchemes
	{
		get => this.AuthenticationSchemes;
		set { }
	}

	public string? Policy { get; set; }
	public string? Roles { get; set; }
}
