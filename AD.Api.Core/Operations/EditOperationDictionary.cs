using AD.Api.Strings.Extensions;

namespace AD.Api.Core.Operations;

public interface IEditOperation
{
    int Count { get; }

    void ApplyToRequest(ModifyRequest request);
}

public abstract class EditOperationDictionary<T> : IEditOperation
{
    private readonly Dictionary<string, T> _dict;

    public int Count => _dict.Count;

    protected EditOperationDictionary(int capacity)
    {
        _dict = new(capacity, StringComparer.OrdinalIgnoreCase);
    }

    protected static void AddValue(DirectoryAttributeModification modification, object value)
    {
        switch (value)
        {
            case string strVal:
                modification.Add(strVal);
                break;

            case byte[] byteArrayVal:
                modification.Add(byteArrayVal);
                break;

            case string[] strArrayVal:
                modification.AddRange(strArrayVal);
                break;

            case Uri urlVal:
                modification.Add(urlVal);
                break;

            default:
                modification.Add(value.ToString().OrEmpty());
                break;
        }
    }
    public void ApplyToRequest(ModifyRequest request)
    {
        foreach (DirectoryAttributeModification modification in this.EnumerateModifications(_dict.Values))
        {
            request.Modifications.Add(modification);
        }
    }
    
    public bool ContainsKey(string propertyName)
    {
        return _dict.ContainsKey(propertyName);
    }
    protected abstract IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<T> values);
    protected static DirectoryAttributeModification GetModification(string propertyName, DirectoryAttributeOperation operation)
    {
        return new DirectoryAttributeModification
        {
            Name = propertyName,
            Operation = operation,
        };
    }
    protected bool TryAdd(string propertyName, T value)
    {
        return _dict.TryAdd(propertyName, value);
    }
}

