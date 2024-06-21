namespace AD.Api.Core.Serialization
{
    public sealed class SerializationSettings
    {
        public string[] GuidAttributes { get; init; } = [];
        public bool WriteIndented { get; init; }
    }
}

