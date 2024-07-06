using System.Runtime.InteropServices;

namespace AD.Api.Components
{
    public static class OneOf<T>
    {
        public static OneOf<T0, T> FromT0<T0>(T0 item0)
        {
            return new(item0);
        }
        public static OneOf<T, T1> FromT1<T1>(T1 item1)
        {
            return new(item1);
        }
    }

    [DebuggerStepThrough]
    [StructLayout(LayoutKind.Auto)]
    [DebuggerDisplay(@"\{IsT0={IsT0}, IsT1={IsT1}, Value={Value}\}")]
    public readonly struct OneOf<T0, T1>
    {
        private readonly int _index;

        public readonly T0? AsT0 { get; }
        public readonly T1? AsT1 { get; }
        [MemberNotNullWhen(true, nameof(AsT0))]
        [MemberNotNullWhen(false, nameof(AsT1))]
        public readonly bool IsT0 { get; }
        [MemberNotNullWhen(false, nameof(AsT0))]
        [MemberNotNullWhen(true, nameof(AsT1))]
        public readonly bool IsT1 { get; }

        public readonly int Index => _index;
        public readonly object Value { get; }

        private OneOf(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            this.Value = obj;
        }
        internal OneOf(T0? item0, T1? item1, int index)
        {
            this.AsT0 = item0;
            this.AsT1 = item1;
            switch (index)
            {
                case 0:
                    this.IsT0 = true;
                    this.IsT1 = false;
                    this.Value = item0!;
                    break;

                case 1:
                    this.IsT0 = false;
                    this.IsT1 = true;
                    this.Value = item1!;
                    break;

                case 2:
                    index = 1;
                    goto case 1;

                default:
                    throw new ArgumentOutOfRangeException(nameof(index));
            }

            _index = index;
        }
        public OneOf(T0 item)
            : this(obj: CastToObject(item))
        {
            this.AsT0 = item;
            this.IsT0 = true;

            this.AsT1 = default;
            this.IsT1 = false;
            _index = 0;
        }
        public OneOf(T1 item)
            : this(obj: CastToObject(item))
        {
            this.AsT1 = item;
            this.IsT1 = true;

            this.AsT0 = default;
            this.IsT0 = false;
            _index = 1;
        }

        private static object CastToObject<T>(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return item;
        }

        public readonly void Match<TState>(TState state, Action<TState, T0> a0, Action<TState, T1> a1)
        {
            switch (_index)
            {
                case 0:
                    a0(state, this.AsT0!);
                    break;

                case 1:
                    a1(state, this.AsT1!);
                    break;

                default:
                    break;
            }
        }
        public TOutput Match<TOutput>(Func<T0, TOutput> f0, Func<T1, TOutput> f1)
        {
            return this.IsT0
                ? f0(this.AsT0)
                : f1(this.AsT1);
        }
        public TOutput Match<TState, TOutput>(TState state, 
            Func<TState, T0, TOutput> f0,
            Func<TState, T1, TOutput> f1)
        {
            return this.IsT0
                ? f0(state, this.AsT0)
                : f1(state, this.AsT1);
        }

        public bool TryGetT0([NotNullWhen(true)] out T0? t0, [NotNullWhen(false)] out T1? t1)
        {
            t0 = this.AsT0;
            t1 = this.AsT1;
            return this.IsT0;
        }
        public bool TryGetT1([NotNullWhen(true)] out T1? t1, [NotNullWhen(false)] out T0? t0)
        {
            t0 = this.AsT0;
            t1 = this.AsT1;
            return this.IsT1;
        }

        public static implicit operator OneOf<T0, T1>(T0 item) => new(item);
        public static implicit operator OneOf<T0, T1>(T1 item) => new(item);
    }
}
