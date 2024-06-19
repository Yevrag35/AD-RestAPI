namespace AD.Api.Components
{
    public static class OneOf<T0>
    {
        public static OneOf<T0, T1> FromT1<T1>(T1 item)
        {
            return new(item);
        }
    }

    [DebuggerStepThrough]
    [DebuggerDisplay(@"\{IsT0={IsT0}, IsT1={IsT1}, Value={Value}\}")]
    public readonly struct OneOf<T0, T1>
    {
        public readonly T0? AsT0 { get; }
        public readonly T1? AsT1 { get; }
        [MemberNotNullWhen(true, nameof(AsT0))]
        [MemberNotNullWhen(false, nameof(AsT1))]
        public readonly bool IsT0 { get; }
        [MemberNotNullWhen(false, nameof(AsT0))]
        [MemberNotNullWhen(true, nameof(AsT1))]
        public readonly bool IsT1 { get; }
        public readonly object Value { get; }

        private OneOf(object? obj)
        {
            ArgumentNullException.ThrowIfNull(obj);
            this.Value = obj;
        }
        public OneOf(T0 item)
            : this(obj: CastToObject(item))
        {
            this.AsT0 = item;
            this.IsT0 = true;

            this.AsT1 = default;
            this.IsT1 = false;
        }
        public OneOf(T1 item)
            : this(obj: CastToObject(item))
        {
            this.AsT1 = item;
            this.IsT1 = true;

            this.AsT0 = default;
            this.IsT0 = false;
        }

        private static object CastToObject<T>(T item)
        {
            ArgumentNullException.ThrowIfNull(item);
            return item;
        }

        public TOutput Match<TState, TOutput>(TState state, 
            Func<TState, T0, TOutput> f0,
            Func<TState, T1, TOutput> f1)
        {
            return this.IsT0
                ? f0(state, this.AsT0)
                : f1(state, this.AsT1);
        }

        public static implicit operator OneOf<T0, T1>(T0 item) => new(item);
        public static implicit operator OneOf<T0, T1>(T1 item) => new(item);
    }
}
