using AD.Api.Components;
using System.Collections;
using System.Runtime.CompilerServices;

namespace AD.Api.Core.Operations;

public interface IAppendableSingleOperation
{
	bool Add(string propertyName, in Either<string, byte[], string[]> value);
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

	protected static void AddValue(DirectoryAttributeModification modification, ObjEither<string, byte[], string[]> oneOf)
	{
		Debug.Assert(oneOf.Index != 0);

		switch (oneOf.Index)
		{
			case 1:
				modification.Add(Unsafe.As<string>(oneOf.Value));
				break;

			case 2:
				modification.Add(Unsafe.As<byte[]>(oneOf.Value));
				break;

			case 3:
				modification.AddRange(Unsafe.As<string[]>(oneOf.Value));
				break;

			default:
				return;
		}
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

