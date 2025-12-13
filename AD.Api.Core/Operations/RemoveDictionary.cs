
using AD.Api.Components;

namespace AD.Api.Core.Operations;

public sealed class RemoveDictionary : EditOperationDictionary<DirectoryAttributeModification>,
	IAppendableSingleOperation,
	IModificationCreator<RemoveDictionary>
{
	public RemoveDictionary()
		: base(1)
	{
	}

	public bool Add(string propertyName, OneOf<string, byte[], string[]> value)
	{
		if (this.ContainsKey(propertyName))
		{
			return false;
		}

		DirectoryAttributeModification mod = GetModification(propertyName, DirectoryAttributeOperation.Delete);
		AddValue(mod, value);

		return this.TryAdd(propertyName, mod);
	}

	protected override IEnumerable<DirectoryAttributeModification> EnumerateModifications(IEnumerable<DirectoryAttributeModification> values)
	{
		return values;
	}

	public static DirectoryAttributeModification CreateModification(string propertyName)
	{
		return GetModification(propertyName, DirectoryAttributeOperation.Delete);
	}
}