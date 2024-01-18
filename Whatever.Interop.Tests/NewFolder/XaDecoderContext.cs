namespace Whatever.Interop.Tests.NewFolder;

public struct XaDecoderContext : IDisposable
{
    public readonly short[] Samples = new short[XaDecoder.SectorMaxSamples  * 4];
    public ushort Channels;
    public uint SampleCount;
    public uint SampleRate; // TODO use that value

    public XaDecoderContext()
    {
    }

    public void Dispose()
    {
    }
}