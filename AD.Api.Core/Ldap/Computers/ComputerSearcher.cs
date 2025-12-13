using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Core.Ldap.Computers;

public interface IComputerSearcher
{
	IActionResult FindOne(SidString computerSid, SearchParameters parameters, IServiceProvider provider);

	/// <summary>
	/// Retrieves a single computer object by its object SID from the specified target domain and returns the result
	/// along with the active connection for sending further queries.
	/// </summary>
	/// <param name="computerSid">The computer object's SID to search for.</param>
	/// <param name="target">The target domain and/or domain controller to send the request to.</param>
	/// <param name="extraProperties"></param>
	/// <returns>
	/// A <see cref="ConnectedResponse"/> object containing the distinguishedName of the found computer object - or -
	/// an <see cref="IActionResult"/> containing the web response result if the operation failed or was unable to
	/// find the computer object.
	/// </returns>
	OneOf<ConnectedResponse, IActionResult> FindOneAndContinue(SidString computerSid, in DomainQuery target, string[]? extraProperties = null);
}

[DependencyRegistration(typeof(IComputerSearcher), Lifetime = ServiceLifetime.Singleton)]
internal sealed class ComputerSearcher : IComputerSearcher
{
	private readonly ILdapFilterService _filterSvc;
	private readonly IRequestService _requestSvc;

	public ComputerSearcher(ILdapFilterService filterSvc, IRequestService requestSvc)
	{
		_filterSvc = filterSvc;
		_requestSvc = requestSvc;
	}

	public IActionResult FindOne(SidString computerSid, SearchParameters parameters, IServiceProvider provider)
	{
		string filter = _filterSvc.GetFilter(computerSid, FilteredRequestType.Computer);
		SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.Computer);

		parameters.ApplyParameters(searchFilter);

		return _requestSvc.FindOne(parameters, provider);
	}
	public OneOf<ConnectedResponse, IActionResult> FindOneAndContinue(SidString computerSid, in DomainQuery target, string[]? extraProperties = null)
	{
		string filter = _filterSvc.GetFilter(computerSid, FilteredRequestType.Computer);
		SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.Computer);
		SearchParameters parameters = new()
		{
			SearchRequest = target.GetRequiredService<IPooledItem<LdapSearchRequest>>(),
			Info = target,
			SizeLimit = 1,
			Scope = SearchScope.Subtree,
		};

		parameters.SetProperties(AttributeConstants.DISTINGUISHED_NAME, extraProperties);
		parameters.ApplyParameters(searchFilter);

		return _requestSvc.FindOneAndContinue(parameters);
	}
}