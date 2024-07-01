namespace AD.Api.Core.Serialization
{
    public sealed class SerializationSettings
    {
        public string[] DateTimeAttributes { get; init; } = [];
        public string[] GuidAttributes { get; init; } = [];
        public bool WriteEnumFlagsAsArray { get; init; }
        public bool WriteIndented { get; init; }
        public bool WriteSimpleObjectClass { get; init; }
    }
}

