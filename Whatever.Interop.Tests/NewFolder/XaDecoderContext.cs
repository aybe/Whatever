namespace Whatever.Interop.Tests.NewFolder;

public struct XaDecoderContext : IDisposable
{
    public readonly NativeBuffer2D<int> History = new(2, 2);
    public readonly NativeBuffer1D<byte> Input = new(2352);
    public readonly short[] Samples = new short[18 * 112 * 4];
    public ushort Channels;
    public uint SampleCount;
    public uint SampleRate; // TODO use that value

    public XaDecoderContext()
    {
    }

    public readonly void Dispose()
    {
        History.Dispose();
        Input.Dispose();
    }
}