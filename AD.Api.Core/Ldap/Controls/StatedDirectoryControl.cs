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

        public bool AddToRequest
        {
            get => _adding;
            set
            {
                _adding = value;
                this.IsCritical = value && _control.IsCritical;
            }
        }

        public StatedDirectoryControl([DisallowNull] T control)
            : base(control.Type, [], false, control.ServerSide)
        {
            _control = control;
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
        public override byte[] GetValue()
        {
            if (!this.AddToRequest)
            {
                return [];
            }

            return _control.GetValue();
        }
    }
}

