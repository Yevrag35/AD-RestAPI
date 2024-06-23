using AD.Api.Attributes;

namespace AD.Api.Statics
{
    /// <summary>
    /// A <see langword="static"/> constant class for the format strings <see cref="Guid"/> structs.
    /// </summary>
    [StaticConstantClass]
    public static class GuidConstants
    {
        public const string B_FORMAT = "B";
        public const string D_FORMAT = "D";
        public const string N_FORMAT = "N";
        public const string P_FORMAT = "P";
        public const string X_FORMAT = "X";
    }
}
