
using AD.Api.Components;

namespace AD.Api.Core.Operations;

public sealed class SetDictionary : EditOperationDictionary<DirectoryAttributeModification>,
    IAppendableSingleOperation,
    IModificationCreator<SetDictionary>
{
    public SetDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, OneOf<string, byte[], string[]> value)
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

    public static DirectoryAttributeModification CreateModification(string propertyName)
    {
        return GetModification(propertyName, DirectoryAttributeOperation.Replace);
    }
}
