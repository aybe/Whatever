using System.Runtime.InteropServices;
using Whatever.Extensions;

namespace Whatever.ISO9660.Extensions;

public sealed class NativeMarshaller<T> : DisposableAsync where T : struct
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

    public override ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }

    public override void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~NativeMarshaller()
    {
        Dispose(false);
    }

    protected override void Dispose(bool disposing)
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