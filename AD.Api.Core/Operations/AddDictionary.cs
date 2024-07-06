using AD.Api.Components;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Operations;

//[CollectionBuilder(typeof(AddDictionary), nameof(Create))]
public sealed class AddDictionary : EditOperationDictionary<DirectoryAttributeModification>, 
    IAppendableSingleOperation,
    IModificationCreator<AddDictionary>
{
    public AddDictionary()
        : base(1)
    {
    }

    public bool Add(string propertyName, OneOf<string, byte[], string[]> value)
    {
        if (this.ContainsKey(propertyName))
        {
            return false;
        }

        DirectoryAttributeModification mod = GetModification(propertyName, DirectoryAttributeOperation.Add);
        AddValue(mod, in value);

        return this.TryAdd(propertyName, mod);
    }
    protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModification> values)
    {
        return values;
    }

    //[SuppressMessage("Style", "IDE0028:Simplify collection initialization", Justification = "This is the builder method.")]
    //public static AddDictionary Create(ReadOnlySpan<DirectoryAttributeModification> span)
    //{
    //    AddDictionary col = new();
    //    foreach (DirectoryAttributeModification mod in span)
    //    {
    //        col.Add(mod.Name, mod);
    //    }

    //    return col;
    //}

    public static DirectoryAttributeModification CreateModification(string propertyName)
    {
        return GetModification(propertyName, DirectoryAttributeOperation.Add);
    }
}

