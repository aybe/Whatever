using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Base class for an asynchronously disposable object.
    /// </summary>
    public abstract class DisposableAsync : Disposable, IAsyncDisposable
    {
        /// <inheritdoc />
        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/3675")]
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Override to control dispose strategy.
        /// </summary>
        /// <remarks>
        ///     Currently, method is a no-operation.
        /// </remarks>
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await new ValueTask().ConfigureAwait(false);
        }
    }
}