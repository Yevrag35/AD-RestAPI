namespace AD.Api.Collections;

/// <summary>
/// Provides a conditional scope-based lock that is acquired only when specified, and releases the lock upon disposal.
/// </summary>
/// <remarks>This ref struct is intended for internal use to manage lock acquisition and release in scenarios
/// where locking may be optional. The lock is released when the instance is disposed, but only if it was acquired.
/// Instances of this type should not be stored or used outside the stack scope in which they are created.</remarks>
[StructLayout(LayoutKind.Auto)]
internal ref struct ConditionalLock
{
	private Lock.Scope _scope;
	private bool _isLocked;
	private bool _isNotDefault;

	/// <summary>
	/// Gets a value indicating whether the <see cref="ConditionalLock"/> is default-initialized.
	/// </summary>
	/// <remarks>
	/// When this is <see langword="true"/>, calling <see cref="Dispose"/> has no effect.
	/// </remarks>
	public readonly bool IsDefault => !_isNotDefault;
	/// <summary>
	/// Gets a value indicating whether the <see cref="ConditionalLock"/> currently holds a lock.
	/// </summary>
	/// <remarks>
	/// When this is <see langword="true"/>, the lock will be released upon calling <see cref="Dispose"/>.
	/// </remarks>
	public readonly bool IsLocked => _isLocked;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConditionalLock"/> struct and acquires the lock if the provided scope is valid.
	/// </summary>
	/// <remarks>After construction, the lock is considered held by this instance. The provided scope is set to its
	/// default value to prevent further use. This constructor is intended for internal use and should not be called
	/// directly by external code.</remarks>
	/// <param name="scope">A scoped reference to the lock scope to be acquired. The reference is consumed and reset during construction.</param>
	internal ConditionalLock(scoped ref Lock.Scope scope)
	{
		_scope = scope;
		scope = default;
		_isLocked = true;
		_isNotDefault = true;
	}

	/// <summary>
	/// Releases all resources used by the current instance and, if a lock is held, disposes the associated lock scope.
	/// </summary>
	/// <remarks>Call this method when you are finished using the instance to ensure that any held locks are
	/// properly released. After calling <see cref="Dispose"/>, the instance should not be used.</remarks>
	/// <exception cref="SynchronizationLockException">The calling thread does not hold the lock.</exception>
	public void Dispose()
	{
		Lock.Scope scope = _scope;
		bool isLocked = _isLocked;
		this = default;

		if (isLocked)
		{
			scope.Dispose();
		}
	}
}