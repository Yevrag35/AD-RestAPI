using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Controls
{
    public static class StatedDirectoryControl
    {
        public static StatedDirectoryControl<T> Create<T>([DisallowNull] T control) where T : DirectoryControl
        {
            return new StatedDirectoryControl<T>(control);
        }
    }
    public sealed class StatedDirectoryControl<T> : DirectoryControl where T : DirectoryControl
    {
        private readonly T _control;
        private bool _adding;
        private readonly Action<T>? _reset;

        public bool AddToRequest
        {
            get => _adding;
            set
            {
                _adding = value;
                this.IsCritical = value && _control.IsCritical;
            }
        }
        [MemberNotNullWhen(true, nameof(_reset))]
        internal bool HasReset { get; }

        public StatedDirectoryControl([DisallowNull] T control, Action<T>? resetAction = null)
            : base(control.Type, [], false, control.ServerSide)
        {
            _control = control;
            this.HasReset = resetAction is not null;
            _reset = resetAction;
        }

        [DebuggerStepThrough]
        public void ChangeState(Action<T> action)
        {
            action(_control);
        }
        [DebuggerStepThrough]
        public void ChangeState<TState>(TState state, Action<TState, T> action)
        {
            action(state, _control);
        }
        public TValue GetControlValue<TValue>(Func<T, TValue> getFunc)
        {
            return getFunc(_control);
        }
        public override byte[] GetValue()
        {
            if (!this.AddToRequest)
            {
                return [];
            }

            return _control.GetValue();
        }

        public void Reset()
        {
            if (this.HasReset)
            {
                _reset(_control);
            }
        }
    }
}

