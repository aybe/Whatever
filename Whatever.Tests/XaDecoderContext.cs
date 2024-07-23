namespace Whatever.Tests;

public sealed class XaDecoderContext
{
    private static int[] Filter1 { get; } = [0, +60, +115, +98, +122];

    private static int[] Filter2 { get; } = [0, 0, -52, -55, -60];

    private int[][] Buffer { get; } = [[0, 0], [0, 0]];

    private short[] Output { get; } = new short[18 * 112 * 4];

    /// <summary>
    ///     Gets the number of channels decoded by last <see cref="Decode" />.
    /// </summary>
    public int Channels { get; private set; }

    /// <summary>
    ///     Gets the frequency decoded by last <see cref="Decode" />.
    /// </summary>
    public int Frequency { get; private set; }

    /// <summary>
    ///     Gets the number of samples decoded by last <see cref="Decode" />.
    /// </summary>
    public int Samples { get; private set; }

    public Span<short> Decode(Span<byte> sector)
    {
        ArgumentOutOfRangeException.ThrowIfNotEqual(sector.Length, 2352);

        var info = sector[19];

        var is8Bit = (info & 0x30) != 0;
        var isStereo = (info & 0x03) != 0;
        var sampleRate = (info & 0x0C) != 0;

        Channels = isStereo
            ? 2
            : 1;

        Frequency = sampleRate
            ? 18900
            : 37800;

        Samples = is8Bit
            ? isStereo
                ? Decode(sector, 8, 2, 0, 2, 0, 0xFF, 8, 24)
                : Decode(sector, 8, 4, 0, 1, 0, 0xFF, 8, 24)
            : isStereo
                ? Decode(sector, 4, 4, 0, 2, 1, 0xF, 12, 28)
                : Decode(sector, 4, 8, 1, 1, 0, 0xF, 12, 28);

        var span = Output.AsSpan(0, Samples * Channels);

        return span;
    }

    private int Decode(
        Span<byte> span, int bits, int blockCount, int blockMask, int channelCount, int channelMask, int sampleMask, int sampleShift, int signedShift)
    {
        var buffer = Buffer;
        var output = Output;
        var sector = span[(12 + 4 + 8)..];
        var offset = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < blockCount; block++)
            {
                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < channelCount; channel++)
                    {
                        var pi = 4 + block * channelCount + channel;
                        var pb = sector[pi];
                        var pr = (pb >> 0) & 0xF;
                        var pf = (pb >> 4) & 0xF;

                        var f0 = Filter1[pf];
                        var f1 = Filter2[pf];

                        var si = 16 + sample * 4 + block * 4 / blockCount + channel * bits / 8;
                        var sj = sector[si];
                        var sk = ((block & blockMask) | (channel & channelMask)) * 4;
                        var sl = (sj >> sk) & sampleMask;
                        var sm = (sl << signedShift) >> signedShift;

                        var sh = buffer[channel];

                        ref var h1 = ref sh[0];
                        ref var h2 = ref sh[1];

                        var sx = (sm << (sampleShift - pr)) + (h2 * f0 + h1 * f1 + 32) / 64;
                        var sy = (short)Math.Clamp(sx, short.MinValue, short.MaxValue);

                        h1 = h2;
                        h2 = sy;

                        output[offset] = sy;

                        offset++;
                    }
                }
            }

            sector = sector[128..];
        }

        var samples = offset / channelCount;

        return samples;
    }
}