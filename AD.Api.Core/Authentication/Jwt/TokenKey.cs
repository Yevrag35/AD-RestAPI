namespace AD.Api.Core.Authentication.Jwt;

public sealed record TokenKey : IEquatable<TokenKey>
{
    public required Guid Id { get; init; }
    public required string Hash { get; init; }

    [SetsRequiredMembers]
    public TokenKey(string userHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userHash);
        this.Id = Guid.NewGuid();
        this.Hash = userHash;
    }
}
