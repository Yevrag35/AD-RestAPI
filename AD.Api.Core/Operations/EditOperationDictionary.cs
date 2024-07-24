using AD.Api.Components;
using System.Collections;

namespace AD.Api.Core.Operations;

public interface IAppendableSingleOperation
{
    bool Add(string propertyName, OneOf<string, byte[], string[]> value);
}
public interface IEditOperation
{
    void ApplyToRequest(ModifyRequest request);
}
public interface IModificationCreator<T>
{
    static abstract DirectoryAttributeModification CreateModification(string propertyName);
}

public abstract class EditOperationDictionary : IEditOperation
{
    public abstract int Count { get; }

    protected static void AddValue(DirectoryAttributeModification modification, in OneOf<string, byte[], string[]> oneOf)
    {
        oneOf.Match(modification,
            a0: (mod, str) => mod.Add(str),
            a1: (mod, bytes) => mod.Add(bytes),
            a2: (mod, array) => mod.AddRange(array));
    }
    public abstract void ApplyToRequest(ModifyRequest request);
}

public abstract class EditOperationDictionary<T> : EditOperationDictionary, IEditOperation, IReadOnlyCollection<T> where T : notnull
{
    private readonly Dictionary<string, T> _dict;

    public sealed override int Count => _dict.Count;

    protected EditOperationDictionary(int capacity)
    {
        _dict = new(capacity, StringComparer.OrdinalIgnoreCase);
    }
    
    public sealed override void ApplyToRequest(ModifyRequest request)
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
    [DebuggerStepThrough]
    public IEnumerator<T> GetEnumerator()
    {
        return _dict.Values.GetEnumerator();
    }
    [DebuggerStepThrough]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }
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

