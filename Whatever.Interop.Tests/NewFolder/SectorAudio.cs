namespace Whatever.Interop.Tests.NewFolder;

public class SectorAudio
{
    public readonly short[] Samples = new short[Globals.SectorMaxSamples];
    public uint SampleCount;
    public uint SampleRate; // TODO use that value
}