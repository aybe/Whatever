namespace Whatever.Interop.Tests.NewFolder;

public class Context : IDisposable
{
    public readonly NativeBuffer2D<short> History = new(2, 2);

    public void Dispose() // TODO dispose pattern
    {
        History.Dispose();
    }
}