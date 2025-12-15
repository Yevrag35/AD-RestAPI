using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap.Filters;
using AD.Api.Core.Ldap.Groups;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Security;
using AD.Api.Pooling;

namespace AD.Api.Core.Ldap.Users;

public partial interface IUserService
{
	IActionResult FindOne(SidString userSid, SearchParameters parameters, IServiceProvider provider);
	/// <summary>
	/// Retrieves a single user object by its object SID from the specified target domain and returns the result
	/// along with the active connection for sending further queries.
	/// </summary>
	/// <param name="userSid">The user object's SID to search for.</param>
	/// <param name="target">The target domain and/or domain controller to send the request to.</param>
	/// <param name="extraProperties"></param>
	/// <returns>
	/// A <see cref="ConnectedResponse"/> object containing the distinguishedName of the found user object - or -
	/// an <see cref="IActionResult"/> containing the web response result if the operation failed or was unable to
	/// find the user object.
	/// </returns>
	ObjEither<ConnectedResponse, IActionResult> FindOneAndContinue(SidString userSid, in DomainQuery target, string[]? extraProperties = null);

	IActionResult ResolveUserGroups(SidString userSid, SearchParameters searchParameters, in DomainQuery target);
}

[DependencyRegistration(typeof(IUserService), Lifetime = ServiceLifetime.Singleton)]
internal sealed partial class UserService : IUserService
{
	private static readonly string[] _groupSearchProperties = [
		AttributeConstants.DISTINGUISHED_NAME,
		AttributeConstants.MEMBER_OF,
	];

	private readonly ILdapFilterService _filterSvc;
	private readonly IGroupService _groupSearcher;
	private readonly IRequestService _requestSvc;
	private readonly CreationService _creationSvc;
	private readonly IDeletionService _deletionSvc;
	private readonly IMoveService _moveSvc;
	private readonly IRenameService _renameSvc;

	public UserService(ILdapFilterService filterSvc, IGroupService groupSearcher, IRequestService requestSvc, CreationService creationSvc, IDeletionService deletionSvc, IMoveService moveSvc, IRenameService renameSvc)
	{
		_filterSvc = filterSvc;
		_groupSearcher = groupSearcher;
		_requestSvc = requestSvc;
		_creationSvc = creationSvc;
		_deletionSvc = deletionSvc;
		_moveSvc = moveSvc;
		_renameSvc = renameSvc;
	}

	public IActionResult FindOne(SidString userSid, SearchParameters parameters, IServiceProvider provider)
	{
		string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
		SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);

		parameters.ApplyParameters(searchFilter);

		return _requestSvc.FindOne(parameters, provider);
	}
	public ObjEither<ConnectedResponse, IActionResult> FindOneAndContinue(SidString userSid, in DomainQuery target, string[]? extraProperties = null)
	{
		string filter = _filterSvc.GetFilter(userSid, FilteredRequestType.User);
		SearchFilterLite searchFilter = SearchFilterLite.Create(filter, FilteredRequestType.User);
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

	public IActionResult ResolveUserGroups(SidString userSid, SearchParameters searchParameters, in DomainQuery target)
	{
		var oneOf = this.FindOneAndContinue(userSid, in target, _groupSearchProperties);
		if (oneOf.TryGetT2(out IActionResult? error, out ConnectedResponse? continuation))
		{
			return error;
		}

		Guid leaseId = searchParameters.Request.RequestId;
		searchParameters.Request.Reset();
		searchParameters.Request.RequestId = leaseId;
		return _groupSearcher.ResolveUserGroups(continuation, searchParameters);
	}
}