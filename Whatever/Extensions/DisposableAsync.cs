using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions;

/// <summary>
///     Base class for an asynchronously disposable object.
/// </summary>
public abstract class DisposableAsync : Disposable, IAsyncDisposable
{
    /// <inheritdoc />
    /// <remarks>
    ///     Current implementation calls <see cref="DisposeAsyncCore" />, <see cref="Disposable.Dispose(bool)" />.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    public virtual async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore().ConfigureAwait(false);

        Dispose(false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Performs asynchronous cleanup of managed resources.
    /// </summary>
    /// <remarks>
    ///     Current implementation calls <see cref="Disposable.DisposeManaged" />.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual ValueTask DisposeAsyncCore()
    {
        DisposeManaged();

        return ValueTask.CompletedTask;
    }
}