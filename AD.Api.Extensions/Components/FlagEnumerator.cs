using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace AD.Api.Components
{
    [StructLayout(LayoutKind.Auto)]
    public ref struct FlagEnumerator<T> where T : unmanaged, Enum
    {
        private int _flags;
        private int _count;
        private T _current;

        public readonly T Current => _current;
        public readonly int Count => _count;

        public FlagEnumerator(T flags)
        {
            ref int intFlag = ref Unsafe.As<T, int>(ref flags);
            _flags = intFlag;

            _count = 0;
            _current = default;
        }

        public bool MoveNext()
        {
            if (0 == _flags)
            {
                return false;
            }

            int flagNum = _flags;
            int currentBit = flagNum & -flagNum; // isolate the rightmost bit
            _current = Unsafe.As<int, T>(ref currentBit);
            _count++;
            _flags &= ~currentBit;  // clear the rightmost bit
            return true;
        }
    }
}

