namespace AD.Api.Core.Schema
{
    public interface ISchemaProperty
    {
        string Name { get; }
        Type RuntimeType { get; }
    }
}

