
using AD.Api.Components;

namespace AD.Api.Core.Operations;

public sealed class ReplaceDictionary : EditOperationDictionary<DirectoryAttributeModification[]>
{
    public ReplaceDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, OneOf<string, byte[], string[]> oldValue, OneOf<string, byte[], string[]> newValue)
    {
        if (this.ContainsKey(propertyName))
        {
            return false;
        }

        DirectoryAttributeModification[] mods = new DirectoryAttributeModification[2];
        DirectoryAttributeModification deleteMod = GetModification(propertyName, DirectoryAttributeOperation.Delete);
        AddValue(deleteMod, oldValue);
        mods[0] = deleteMod;

        DirectoryAttributeModification addMod = GetModification(propertyName, DirectoryAttributeOperation.Add);
        AddValue(addMod, newValue);

        mods[1] = addMod;

        return this.TryAdd(propertyName, mods);
    }

    protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModification[]> collections)
    {
        return collections.SelectMany(x => x);
    }
}

