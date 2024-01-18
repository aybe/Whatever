namespace Whatever.Interop.Tests.NewFolder;

public class XaDecoderContext
{
    public readonly short[] Samples = new short[XaDecoder.SectorMaxSamples  * 4];
    public ushort Channels;
    public uint SampleCount;
    public uint SampleRate; // TODO use that value
}