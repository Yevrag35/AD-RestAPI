
namespace AD.Api.Serialization.Json;

public sealed class ExclusionaryJsonDictionary : JsonDictionary
{
	private readonly HashSet<string> _onlyKeys;

	public ExclusionaryJsonDictionary(IEnumerable<string> onlyKeys)
		: base(onlyKeys.TryGetNonEnumeratedCount(out int count) ? count : 1)
	{
		_onlyKeys = new(onlyKeys, StringComparer.OrdinalIgnoreCase);
	}

	protected override void AddPair(string key, object? value)
	{
		if (_onlyKeys.Contains(key))
		{
			base.AddPair(key, value);
		}
	}
	protected override void SetPair(string key, object? value)
	{
		if (_onlyKeys.Contains(key))
		{
			base.SetPair(key, value);
		}
	}
}
