
namespace AD.Api.Core.Operations;

public sealed class ClearDictionary : EditOperationDictionary<DirectoryAttributeModification>,
	IModificationCreator<ClearDictionary>
{
	public ClearDictionary()
		: base(1)
	{
	}

	public bool Add(string propertyName)
	{
		if (this.ContainsKey(propertyName))
		{
			return false;
		}

		DirectoryAttributeModification mod = GetModification(propertyName, DirectoryAttributeOperation.Delete);
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
