using AD.Api.Core;
using AD.Api.Core.Ldap;
using AD.Api.Core.Ldap.Results;
using AD.Api.Core.Ldap.Users;
using AD.Api.Core.Security;
using AD.Api.Spans;
using AD.Api.Statics;

namespace AD.Api.Core.Extensions.Results;

public static class CreatedToResultExtensions
{
    public static CreatedObject ToCreatedObject(this ResultEntry entry, [ConstantExpected] string createdAt, in DomainQuery target)
    {
        ConnectionContext context = target.GetRequiredService<IConnectionService>().RegisteredConnections[target.Domain];

        SidString sid = new SidString((byte[])entry[AttributeConstants.OBJECT_SID]);

        return new CreatedObject
        {
            DistinguishedName = entry.DistinguishedName,
            Domain = context.DomainName,
            Location = GetLocation(in target, sid, createdAt),
            ObjectSid = sid,
        };
    }

    private static string GetLocation(in DomainQuery target, SidString sid, [ConstantExpected] string createdAt)
    {
        Span<char> chars = stackalloc char[target.UrlQueryLength + 1 + sid.Value.Length + createdAt.Length];
        int pos = 0;

        createdAt.CopyToSlice(chars, ref pos);

        sid.CopyToSlice(chars, ref pos);
        if (target != DomainQuery.Default)
        {
            chars[pos++] = CharConstants.QUESTION;
            target.AppendAsQuery(chars.Slice(pos), out int written);
            pos += written;
        }

        return new string(chars.Slice(0, pos));
    }
}