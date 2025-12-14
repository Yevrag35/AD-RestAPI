using AD.Api.Collections.Extensions;
using AD.Api.Components;
using AD.Api.Core.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace AD.Api.Core.Authentication.Jwt;

public interface IJwtService
{
	bool IsFunctional { get; }
	TimeProvider Clock { get; }

	Either<BearerToken, IActionResult> CreateToken(IJwtLogin loginRequest);
	Either<BearerToken, IResult> CreateTokenMinimal(IJwtLogin loginRequest);
}


internal sealed class JwtCache : IJwtService
{
	private readonly ConcurrentDictionary<string, TokenKey> _keys;
	private readonly JwtHandler _handler;
	private readonly IMemoryCache _cache;
	private readonly TimeSpan _lifetime;
	private readonly TimeSpan _renewThreshold;
	public TimeProvider Clock { get; }
	public bool IsFunctional => true;

	public JwtCache(JwtHandler handler, TimeProvider clock, IMemoryCache cache, CustomJwtSettings settings)
	{
		_handler = handler;
		this.Clock = clock;
		_cache = cache;
		_lifetime = settings.TokenLifetime;
		_renewThreshold = settings.RenewalThreshold;

		_keys = settings.RBAC
			.Users
				.Select(x => KeyValuePair.Create(x.UserHash, new TokenKey(x.UserHash)))
				.ToConcurrentDictionary(StringComparer.Ordinal);
	}

	public BearerToken AddToken(AuthorizedUser user, BearerToken token)
	{
		TokenKey tokenKey = _keys.GetOrAdd(user.UserHash, CreateTokenKey);

		return _cache.Set(tokenKey, token, new MemoryCacheEntryOptions
		{
			AbsoluteExpiration = token.Expires - (_renewThreshold / 2),
			Priority = CacheItemPriority.High,
			Size = 10L,
		});
	}
	public Either<BearerToken, IActionResult> CreateToken(IJwtLogin loginRequest)
	{
		var oneOf = _handler.CreateToken(loginRequest);
		return oneOf.Match(this,
			f1: (result, cache) => Either.FromT1<BearerToken, IActionResult>(cache.AddToken(result.Item2, result.Item1)),
			f2: (fail, cache) => Either.FromT2<BearerToken, IActionResult>(fail));
	}
	public Either<BearerToken, IResult> CreateTokenMinimal(IJwtLogin loginRequest)
	{
		var oneOf = _handler.CreateTokenMinimal(loginRequest);
		return oneOf.Match(this,
			f1: (result, cache) => Either.FromT1<BearerToken, IResult>(cache.AddToken(result.Item2, result.Item1)),
			f2: (fail, cache) => Either.FromT2<BearerToken, IResult>(fail));
	}
	private static TokenKey CreateTokenKey(string key)
	{
		return new(key);
	}
	public DateTimeOffset GetExpirationStamp()
	{
		return this.Clock.GetUtcNow().Add(_lifetime);
	}
	public bool IsExpired([NotNullWhen(false)] BearerToken? token)
	{
		if (token is null)
		{
			return true;
		}

		TimeSpan expiration = token.Expires - this.Clock.GetUtcNow() - _renewThreshold;
		return expiration <= TimeSpan.Zero;
	}
	public bool TryGetToken(AuthorizedUser user, [NotNullWhen(true)] out BearerToken? token)
	{
		return _keys.TryGetValue(user.UserHash, out token, out TokenKey? tokenKey)
			   &&
			   _cache.TryGetValueOrRemove(tokenKey, out token)
			   &&
			   !this.IsExpired(token);
	}

	Either<BearerToken, IActionResult> IJwtService.CreateToken(IJwtLogin loginRequest)
	{
		throw new NotImplementedException();
	}

	
}
