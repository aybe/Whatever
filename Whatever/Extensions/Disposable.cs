using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions;

/// <summary>
///     Base class for a disposable object.
/// </summary>
public abstract class Disposable : IDisposable
{
    private bool IsDisposed { get; set; }

    /// <inheritdoc />
    public virtual void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    ///     Performs cleanup of managed and unmanaged resources.
    /// </summary>
    /// <remarks>
    ///     Current implementation calls <see cref="DisposeManaged" />, <see cref="DisposeNative" />.
    /// </remarks>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        DisposeNative();

        if (disposing)
        {
            DisposeManaged();
        }

        IsDisposed = true;
    }

    /// <summary>
    ///     Performs cleanup of managed resources.
    /// </summary>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual void DisposeManaged()
    {
    }

    /// <summary>
    ///     Performs cleanup of unmanaged resources.
    /// </summary>
    [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
    protected virtual void DisposeNative()
    {
    }

    /// <inheritdoc />
    ~Disposable()
    {
        Dispose(false);
    }

    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(IsDisposed, this);
    }
}