namespace AD.Api.Actions
{
    public static class StatedCallback
    {
        public static StatedCallback<TState, TOutput> Create<TState, TOutput>(TState state, Func<TState, TOutput> callback)
        {
            return new StatedCallback<TState, TOutput>(state, callback);
        }
    }
    public sealed class StatedCallback<TState, TOutput> : IStatedCallback<TOutput>
    {
        private readonly Func<TState, TOutput> _callback;
        private readonly TState _state;

        public StatedCallback(TState state, Func<TState, TOutput> callback)
        {
            _callback = callback;
            _state = state;
        }

        public TOutput Invoke()
        {
            return _callback.Invoke(_state);
        }
    }
}
