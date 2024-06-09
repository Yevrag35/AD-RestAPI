using System.Numerics;

namespace System
{
    /// <summary>
    /// Extension methods for unmanaged types.
    /// </summary>
    public static class UnmanagedExtensions
    {
        /// <summary>
        /// Returns the length of how many <see cref="char"/> elements make 
        /// up <typeparamref name="T"/> value if it were formatted as a 
        /// <see cref="string"/> instance without actually converting it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="n"></param>
        /// <returns>
        /// The number of <see cref="char"/> elements that would make up the <typeparamref name="T"/> value.
        /// </returns>
        [DebuggerStepThrough]
        public static int GetLength<T>(this T n) where T : INumber<T>, IMinMaxValue<T>
        {
            if (T.IsZero(n))
            {
                return 1;
            }

            int length = 0;
            // For negative numbers, add one to count for the '-' sign.
            if (T.IsNegative(n))
            {
                length++;
                AdjustNumber(ref n);
            }

            // Compute floor of log base 10 of the number.
            int flooredLogBase10 = (int)Math.Floor(Math.Log10(double.CreateChecked(n)));

            // Add the number of digits in the absolute value of the number.
            // Since we're not initializing length to 1 anymore, add back only the calculated digits.
            length += flooredLogBase10 + 1;

            return length;
        }

        private static void AdjustNumber<T>(ref T number) where T : INumber<T>, IMinMaxValue<T>
        {
            if (T.MinValue.Equals(number))
            {
                number = T.MaxValue;
            }
            else
            {
                number = T.Abs(number);
            }
        }
    }
}