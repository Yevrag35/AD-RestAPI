using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Settings
{
    public sealed class SerializationSettings
    {
        public enum JsonReferenceHandling
        {
            None,
            Preserve,
            Ignore
        }

        public JsonNumberHandling NumberHandling { get; init; } = JsonNumberHandling.AllowReadingFromString;
        public bool PropertyNameCaseInsensitive { get; init; } = true;
        public JsonReferenceHandling ReferenceHandling { get; init; } = JsonReferenceHandling.Preserve;
        public bool WriteIndented { get; init; } = true;

        public void SetJsonOptions(JsonSerializerOptions options)
        {
            options.NumberHandling = this.NumberHandling;
            options.ReferenceHandler = ResolveHandler(this.ReferenceHandling);
            options.PropertyNameCaseInsensitive = this.PropertyNameCaseInsensitive;
            options.WriteIndented = this.WriteIndented;
        }

        private static ReferenceHandler? ResolveHandler(JsonReferenceHandling value)
        {
            switch (value)
            {
                case JsonReferenceHandling.Preserve:
                    return ReferenceHandler.Preserve;

                case JsonReferenceHandling.Ignore:
                    return ReferenceHandler.IgnoreCycles;

                case JsonReferenceHandling.None:
                    goto default;

                default:
                    return null;
            }
        }
    }
}
