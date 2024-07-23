using System.Runtime.InteropServices;
using Whatever.Extensions;

namespace Whatever.Interop;

public sealed class NativeMarshal<T> : Disposable where T : struct
{
    public NativeMarshal(T structure = default)
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

    protected override void DisposeNative()
    {
        Marshal.FreeHGlobal(Pointer);
    }
}