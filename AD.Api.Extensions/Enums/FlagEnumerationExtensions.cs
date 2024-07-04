namespace AD.Api.Enums;

public static class FlagEnumerationExtensions
{
    public static FlagEnumerator<T> EnumerateFlags<T>(this T value) where T : unmanaged, Enum
    {
        ValidateIsEnumerable<T>();
        return new FlagEnumerator<T>(value);
    }

    [Conditional("DEBUG")]
    private static void ValidateIsEnumerable<T>() where T : unmanaged, Enum
    {
        Type type = typeof(T);
        if (!type.IsDefined(typeof(FlagsAttribute)))
        {
            throw new InvalidOperationException("Type must be marked with the FlagsAttribute in order to enumerate.");
        }
        else if (!typeof(int).Equals(type.GetEnumUnderlyingType()))
        {
            throw new InvalidOperationException("Flag enumeration can only be performed on an integer-based enumeration.");
        }
    }
}
