namespace AD.Api.Core.Serialization;

[StructLayout(LayoutKind.Auto)]
public ref struct SerializationContext
{
	private readonly IServiceProvider _provider;
	private readonly JsonSerializerOptions _options;

	public ReadOnlySpan<char> AttributeName
	{
		readonly get;
		internal set;
	}
	public readonly JsonSerializerOptions Options => _options;
	public readonly IServiceProvider Services => _provider;
	public object Value
	{
		readonly get;
		internal set;
	}

	internal SerializationContext(JsonSerializerOptions options, IServiceProvider scopedProvider)
	{
		this.AttributeName = default;
		_provider = scopedProvider;
		_options = options;
		this.Value = string.Empty;
	}
}

