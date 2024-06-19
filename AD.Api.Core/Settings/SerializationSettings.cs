namespace AD.Api.Core.Settings
{
    public sealed class SerializationSettings
    {
        public string[] GuidAttributes { get; init; } = [];
        public bool WriteIndented { get; init; }
    }
}

