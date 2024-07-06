
namespace AD.Api.Core.Operations;

public sealed class SetDictionary : EditOperationDictionary<DirectoryAttributeModification>
{
    public SetDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, object value)
    {
        if (this.ContainsKey(propertyName))
        {
            return false;
        }

        DirectoryAttributeModification mod = GetModification(propertyName, DirectoryAttributeOperation.Replace);
        AddValue(mod, value);

        return this.TryAdd(propertyName, mod);
    }

    protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModification> values)
    {
        return values;
    }
}
