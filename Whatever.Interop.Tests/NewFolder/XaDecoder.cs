namespace Whatever.Interop.Tests.NewFolder;

public static class XaDecoder
{
    public static readonly short[] PositiveFilters = [0, +60, +115, +98, +122];

    public static readonly short[] NegativeFilters = [0, 0, -52, -55, -60];

    public static void Decode(Span<byte> src, ref XaDecoderContext ctx)
    {
        var isStereo = (src[19] & 0x3) != 0;
        var is8Bit = (src[19] & 0x30) != 0;
        var sampleRate = (src[19] & 0xC) != 0 ? 18900 : 37800;

        ctx.Channels = (ushort)(isStereo ? 2 : 1);
        ctx.SampleRate = (uint)sampleRate;

        if (is8Bit)
        {
            if (isStereo)
            {
                ctx.SampleCount = (uint)Decode(ref ctx, 2, 2, 8, 0, 0, 0xFF, 24, 8);
            }
            else
            {
                ctx.SampleCount = (uint)Decode(ref ctx, 1, 4, 8, 0, 0, 0xFF, 24, 8);
            }
        }
        else
        {
            if (isStereo)
            {
                ctx.SampleCount = (uint)Decode(ref ctx, 2, 4, 4, 0, 1, 0x0F, 28, 12);
            }
            else
            {
                ctx.SampleCount = (uint)Decode(ref ctx, 1, 8, 4, 1, 0, 0x0F, 28, 12);
            }
        }
    }

    private static int Decode(
        ref XaDecoderContext ctx, int channels, int blocks, int bits, int blockMask, int channelMask, int sampleMask, int signShift, int sampleShift)
    {
        var source = ctx.Input.Span[(12 + 4 + 8)..];

        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < blocks; block++)
            {
                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < channels; channel++)
                    {
                        var si = 4 + block * channels + channel;
                        var sp = source[si];
                        var sr = sp & 0xF;
                        var sf = (sp & 0x30) >> 4;
                        var f0 = PositiveFilters[sf];
                        var f1 = NegativeFilters[sf];

                        var k = 16 + sample * 4 + block * 4 / blocks + channel * bits / 8;
                        var t = source[k];
                        var z = ((block & blockMask) | (channel & channelMask)) * 4;
                        var u = (t >> z) & sampleMask;
                        var v = (u << signShift) >> signShift;

                        ref var x = ref ctx.History[channel][1];
                        ref var y = ref ctx.History[channel][0];

                        var s = (v << (sampleShift - sr)) + (y * f0 + x * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        x = y;
                        y = s;
                        ctx.Output[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }
}