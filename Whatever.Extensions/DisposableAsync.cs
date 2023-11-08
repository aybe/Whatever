using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Whatever.Extensions
{
    public abstract class DisposableAsync : Disposable
    {
        protected virtual async ValueTask DisposeAsyncCore()
        {
            await new ValueTask().ConfigureAwait(false);
        }

        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "https://github.com/dotnet/roslyn-analyzers/issues/3675")]
        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCore().ConfigureAwait(false);

            GC.SuppressFinalize(this);
        }
    }
}