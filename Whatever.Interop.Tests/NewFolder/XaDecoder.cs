namespace Whatever.Interop.Tests.NewFolder;

public static class XaDecoder
{
    public static readonly int[][] History = [[0, 0], [0, 0]];

    public static void Decode(Span<byte> src, SectorAudio output)
    {
        var isStereo = (src[19] & 0x3) != 0;
        var is8Bit = (src[19] & 0x30) != 0;
        var sampleRate = (src[19] & 0xC) != 0 ? 18900 : 37800;

        output.Channels = (ushort)(isStereo ? 2 : 1);
        output.SampleRate = (uint)sampleRate;

        Globals.WriteLine(
            $"{nameof(isStereo)}: {isStereo}, " +
            $"{nameof(is8Bit)}: {is8Bit}, " +
            $"{nameof(sampleRate)}: {sampleRate}"
        );

        src = src[(12 + 4 + 8)..];

        if (is8Bit)
        {
            if (isStereo)
            {
                output.SampleCount = (uint)Decode82(src, output.Samples, 2, 2, 8, 0, 0, 0xFF);
            }
            else
            {
                output.SampleCount = (uint)Decode81(src, output.Samples, 1, 4, 8, 0, 0, 0xFF);
            }
        }
        else
        {
            if (isStereo)
            {
                output.SampleCount = (uint)Decode42(src, output.Samples, 2, 4, 4, 0, 1, 0x0F);
            }
            else
            {
                output.SampleCount = (uint)Decode41(src, output.Samples, 1, 8, 4, 1, 0, 0x0F);
            }
        }
    }

    private static int Decode41(
        Span<byte> source, Span<short> target, int channels, int blocks, int bits, int blockMask, int channelMask, int sampleMask)
    {
        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < blocks; block++) // 8
            {
                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < channels; channel++)
                    {
                        var si = 4 + block * channels + channel;
                        var sp = source[si];

                        Unpack(sp, out var sr, out var sf, out var f0, out var f1);

                        Print(group, block, sample, channel, si, sp, sr, sf, f0, f1);

                        var k = 16 + sample * 4 + block * 4 / blocks + channel * bits / 8;
                        var t = source[k];
                        var z = ((block & blockMask) | (channel & channelMask)) * 4;
                        var u = (t >> z) & sampleMask;
                        var v = (u << 28) >> 28;

                        ref var x = ref History[channel][0];
                        ref var y = ref History[channel][1];

                        var s = (v << (12 - sr)) + (x * f0 + y * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        y = x;
                        x = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }

    private static int Decode42(
        Span<byte> source, Span<short> target, int channels, int blocks, int bits, int blockMask, int channelMask, int sampleMask)
    {
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

                        Unpack(sp, out var sr, out var sf, out var f0, out var f1);

                        Print(group, block, sample, channel, si, sp, sr, sf, f0, f1);

                        var k = 16 + sample * 4 + block * 4 / blocks + channel * bits / 8;
                        var t = source[k];
                        var z = ((block & blockMask) | (channel & channelMask)) * 4;
                        var u = (t >> z) & sampleMask;
                        var v = (u << 28) >> 28;

                        ref var x = ref History[channel][0];
                        ref var y = ref History[channel][1];

                        var s = (v << (12 - sr)) + (x * f0 + y * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        y = x;
                        x = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }

    private static int Decode81(
        Span<byte> source, Span<short> target, int channels, int blocks, int bits, int blockMask, int channelMask, int sampleMask)
    {
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

                        Unpack(sp, out var sr, out var sf, out var f0, out var f1);

                        Print(group, block, sample, channel, si, sp, sr, sf, f0, f1);

                        var k = 16 + sample * 4 + block * 4 / blocks + channel * bits / 8;
                        var t = source[k];
                        var z = ((block & blockMask) | (channel & channelMask)) * 4;
                        var u = (t >> z) & sampleMask;
                        var v = (u << 24) >> 24;

                        ref var x = ref History[channel][1];
                        ref var y = ref History[channel][0];

                        var s = (v << (8 - sr)) + (y * f0 + x * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        x = y;
                        y = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }

    private static int Decode82(
        Span<byte> source, Span<short> target, int channels, int blocks, int bits, int blockMask, int channelMask, int sampleMask)
    {
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

                        Unpack(sp, out var sr, out var sf, out var f0, out var f1);

                        Print(group, block, sample, channel, si, sp, sr, sf, f0, f1);

                        var k = 16 + sample * 4 + block * 4 / blocks + channel * bits / 8;
                        var t = source[k];
                        var z = ((block & blockMask) | (channel & channelMask)) * 4;
                        var u = (t >> z) & sampleMask;
                        var v = (u << 24) >> 24;

                        ref var x = ref History[channel][1];
                        ref var y = ref History[channel][0];

                        var s = (v << (8 - sr)) + (y * f0 + x * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        x = y;
                        y = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }

    private static void Print(int group, int block, int sample, int channel, int si, byte sp, int sr, int sf, short f0, short f1)
    {
        Globals.WriteLine(
            $"{nameof(group)}: {group}, " +
            $"{nameof(block)}: {block}, " +
            $"{nameof(sample)}: {sample}, " +
            $"{nameof(channel)}: {channel}, " +
            $"{nameof(si)}: {si}, " +
            $"{nameof(sp)}: 0x{sp:X2}, " +
            $"{nameof(sr)}: {sr}, " +
            $"{nameof(sf)}: {sf}, " +
            $"{nameof(f0)}: {f0,3}, " +
            $"{nameof(f1)}: {f1,3}");
    }

    private static void Unpack(byte sp, out int sr, out int sf, out short f0, out short f1)
    {
        sr = sp & 0xF;
        sf = (sp & 0x30) >> 4;
        f0 = Globals.PositiveFilters[sf];
        f1 = Globals.NegativeFilters[sf];
    }
}