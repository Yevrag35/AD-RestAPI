
namespace AD.Api.Core.Operations;

public sealed class ReplaceDictionary : EditOperationDictionary<DirectoryAttributeModificationCollection>
{
    public ReplaceDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, object oldValue, object newValue)
    {
        if (this.ContainsKey(propertyName))
        {
            return false;
        }

        DirectoryAttributeModificationCollection mods = [];
        DirectoryAttributeModification deleteMod = GetModification(propertyName, DirectoryAttributeOperation.Delete);
        AddValue(deleteMod, oldValue);
        mods.Add(deleteMod);

        DirectoryAttributeModification addMod = GetModification(propertyName, DirectoryAttributeOperation.Add);
        AddValue(addMod, newValue);

        mods.Add(addMod);

        return this.TryAdd(propertyName, mods);
    }

    protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModificationCollection> collections)
    {
        return collections.SelectMany(x => x.Cast<DirectoryAttributeModification>());
    }
}

