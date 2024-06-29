namespace AD.Api.Core.Authentication.Jwt
{
    public interface IJwtLogin
    {
        string Key { get; }
        string UserName { get; }
    }
}

