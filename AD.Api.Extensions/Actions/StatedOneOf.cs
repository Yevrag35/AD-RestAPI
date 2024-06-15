using OneOf;

namespace AD.Api.Actions
{
    public readonly struct StatedOneOf<T0, T1> : IOneOf
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly OneOf<T0, T1> _oneOf;

        [MemberNotNullWhen(true, nameof(Item0))]
        public readonly bool IsT0 => _oneOf.IsT0;
        [MemberNotNullWhen(true, nameof(Item1))]
        public readonly bool IsT1 => _oneOf.IsT1;
        public readonly T0 Item0 => _oneOf.AsT0;
        public readonly T1 Item1 => _oneOf.AsT1;

        public readonly int Index => _oneOf.Index;
        public readonly object Value => _oneOf.Value;

        public StatedOneOf(OneOf<T0, T1> oneOf)
        {
            _oneOf = oneOf;
        }

        public readonly TOutput Match<TState, TOutput>(TState state, Func<TState, T0, TOutput> f0, Func<TState, T1, TOutput> f1)
        {
            return this.IsT0
                ? f0(state, this.Item0)
                : f1(state, this.Item1);
        }

        public static implicit operator StatedOneOf<T0, T1>(OneOf<T0, T1> oneOf) => new(oneOf);
        public static implicit operator StatedOneOf<T0, T1>(T0 item) => new(item);
        public static implicit operator StatedOneOf<T0, T1>(T1 item) => new(item);
    }
}

