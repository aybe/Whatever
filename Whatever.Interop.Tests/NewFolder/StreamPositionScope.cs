namespace Whatever.Interop.Tests.NewFolder;

public readonly struct StreamPositionScope : IDisposable
{
    private readonly long Position;
    private readonly Stream Stream;

    public StreamPositionScope(Stream stream, long? position = null)
    {
        (Stream = stream).Position = Position = position ?? stream.Position;
    }

    public void Dispose()
    {
        Stream.Position = Position;
    }
}