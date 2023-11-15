using System;
using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Base class for a disposable object.
    /// </summary>
    public abstract class Disposable : IDisposable
    {
        /// <summary>
        ///     Gets or sets whether this instance has been disposed.
        /// </summary>
        protected bool IsDisposed { get; set; }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Override to manually control dispose strategy.
        /// </summary>
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
        ///     Override to dispose managed objects.
        /// </summary>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual void DisposeManaged()
        {
        }

        /// <summary>
        ///     Override to dispose native objects.
        /// </summary>
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual void DisposeNative()
        {
        }

        /// <summary>
        ///     Finalizer.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
        }
    }
}