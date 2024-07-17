using System.Runtime.InteropServices;

namespace Whatever.ISO9660.Extensions;

public sealed class NativeMarshaller<T> : IDisposable, IAsyncDisposable where T : struct
{
    private bool Disposed;

    public NativeMarshaller(T structure = default)
    {
        Length = Marshal.SizeOf<T>();

        Pointer = Marshal.AllocHGlobal(Length);

        try
        {
            Marshal.StructureToPtr(structure, Pointer, false);
        }
        catch (Exception)
        {
            Dispose();
            throw;
        }
    }

    public int Length { get; }

    public nint Pointer { get; }

    public T Structure
    {
        get
        {
            ThrowIfDisposed();

            return Marshal.PtrToStructure<T>(Pointer);
        }
        set
        {
            ThrowIfDisposed();

            Marshal.StructureToPtr(value, Pointer, true);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);
    }

    ~NativeMarshaller()
    {
        Dispose(false);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        Marshal.FreeHGlobal(Pointer);

        if (disposing)
        {
            // NOP
        }

        Disposed = true;
    }
}