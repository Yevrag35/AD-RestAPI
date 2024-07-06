
namespace AD.Api.Core.Operations;

public sealed class AddDictionary : EditOperationDictionary<DirectoryAttributeModification>
{
    public AddDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, object value)
    {
        if (this.ContainsKey(propertyName))
        {
            return false;
        }

        DirectoryAttributeModification mod = GetModification(propertyName, DirectoryAttributeOperation.Add);
        AddValue(mod, value);

        return this.TryAdd(propertyName, mod);
    }
    protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModification> values)
    {
        return values;
    }
}

