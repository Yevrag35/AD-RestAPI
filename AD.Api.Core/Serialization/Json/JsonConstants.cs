using AD.Api.Attributes;

namespace AD.Api.Core.Serialization.Json
{
    [StaticConstantClass]
    public static class JsonConstants
    {
        public const string ContentType = "application/json";
        public const string ContentTypeWithCharset = ContentType + "; charset=utf-8";
    }
}

