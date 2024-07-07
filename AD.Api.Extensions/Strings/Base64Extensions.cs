using System.Numerics;

namespace AD.Api.Strings;

public static class Base64Extensions
{
    public static int GetByteLength(ReadOnlySpan<char> base64Text)
    {
        if (base64Text.IsEmpty)
        {
            return 0;
        }

        return CalculateLength(base64Text.Length);
    }
    public static int GetByteLength(int length)
    {
        return length switch
        {
            > 0 => CalculateLength(in length),
            _ => 0,
        };
    }
    public static int GetByteLengthFrom<T>(T length) where T : struct, INumber<T>
    {
        if (T.IsZero(length) || T.IsNegative(length))
        {
            return 0;
        }

        int intLength = (int)Math.Ceiling(double.CreateChecked(length));
        return CalculateLength(in intLength);
    }
    private static int CalculateLength(in int length)
    {
        return ((length * 3) + 3) / 4;
    }
}