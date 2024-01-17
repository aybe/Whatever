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
                output.SampleCount = (uint)Decode82(src, output.Samples);
            }
            else
            {
                output.SampleCount = (uint)Decode81(src, output.Samples);
            }
        }
        else
        {
            if (isStereo)
            {
                output.SampleCount = (uint)Decode42(src, output.Samples);
            }
            else
            {
                output.SampleCount = (uint)Decode41(src, output.Samples);
            }
        }
    }

    private static int Decode41(Span<byte> source, Span<short> target)
    {
        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < 8; block++)
            {
                var sp = source[4 + block];
                var sr = sp & 0xF;
                var sf = (sp & 0x30) >> 4;
                var f0 = Globals.PositiveFilters[sf];
                var f1 = Globals.NegativeFilters[sf];

                Globals.WriteLine(
                    $"{nameof(block)}: {block}, {nameof(sp)}: 0x{sp:X2}, {nameof(sr)}: {sr}, {nameof(sf)}: {sf}, {nameof(f0)}: {f0,3}, {nameof(f1)}: {f1,3}");

                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < 1; channel++)
                    {
                        var k = 16 + sample * 4 + block / 2;
                        var t = (int)source[k];
                        var u = (t >> ((block & 1) * 4)) & 0xF;
                        var v = (u << 28) >> 28;

                        ref var old = ref History[channel][0];
                        ref var older = ref History[channel][1];
                        var s = (v << (12 - sr)) + (old * f0 + older * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        older = old;
                        old = s;
                        target[index++] = (short)s;
                        Globals.WriteLine(
                            $"{nameof(group)}: {group}, " +
                            $"{nameof(block)}: {block}, " +
                            $"{nameof(sample)}: {sample}, " +
                            //$"{nameof(channel)}: {channel}, " +
                            $"{nameof(k)}: {k}, " +
                            //$"{nameof(t)}: 0x{t:X2}, " +
                            //$"{nameof(u)}: 0x{u:X}, " +
                            //$"{nameof(v)}: {v}, " +
                            $"");
                    }
                }
            }

            Globals.WriteLine(null);

            source = source[128..];
        }

        return index;
    }

    private static int Decode42(Span<byte> source, Span<short> target)
    {
        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < 8; block++)
            {
                var sp = source[4 + block];
                var sr = sp & 0xF;
                var sf = (sp & 0x30) >> 4;
                var f0 = Globals.PositiveFilters[sf];
                var f1 = Globals.NegativeFilters[sf];

                Globals.WriteLine(
                    $"{nameof(block)}: {block}, {nameof(sp)}: 0x{sp:X2}, {nameof(sr)}: {sr}, {nameof(sf)}: {sf}, {nameof(f0)}: {f0,3}, {nameof(f1)}: {f1,3}");

                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < 1; channel++)
                    {
                        var k = 16 + sample * 4 + block / 2;
                        var t = (int)source[k];
                        var u = (t >> ((block & 1) * 4)) & 0xF;
                        var v = (u << 28) >> 28;

                        ref var old = ref History[channel][0];
                        ref var older = ref History[channel][1];
                        var s = (v << (12 - sr)) + (old * f0 + older * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        older = old;
                        old = s;
                        target[index++] = (short)s;
                        Globals.WriteLine(
                            $"{nameof(group)}: {group}, " +
                            $"{nameof(block)}: {block}, " +
                            $"{nameof(sample)}: {sample}, " +
                            //$"{nameof(channel)}: {channel}, " +
                            $"{nameof(k)}: {k}, " +
                            //$"{nameof(t)}: 0x{t:X2}, " +
                            //$"{nameof(u)}: 0x{u:X}, " +
                            //$"{nameof(v)}: {v}, " +
                            $"");
                    }
                }
            }

            Globals.WriteLine(null);

            source = source[128..];
        }

        return index;
    }

    private static int Decode81(Span<byte> source, Span<short> target)
    {
        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            Globals.WriteLine($"{nameof(group)}: {group}");

            for (var block = 0; block < 4; block++)
            {
                var sp = source[4 + block];
                var sr = sp & 0xF;
                var sf = (sp & 0x30) >> 4;
                var f0 = Globals.PositiveFilters[sf];
                var f1 = Globals.NegativeFilters[sf];

                Globals.WriteLine(
                    $"\t{nameof(block)}: {block}, " +
                    $"{nameof(sp)}: 0x{sp:X2}, " +
                    $"{nameof(sr)}: {sr}, " +
                    $"{nameof(sf)}: {sf}, " +
                    $"{nameof(f0)}: {f0,3}, " +
                    $"{nameof(f1)}: {f1,3}"
                );

                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < 1; channel++)
                    {
                        ref var older = ref History[channel][1];
                        ref var old = ref History[channel][0];

                        var k = 16 + block * 1 + sample * 4 + channel;
                        int t = source[k];
                        t = (t << 24) >> 24;
                        var s = (t << (8 - sr)) + (old * f0 + older * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        older = old;
                        old = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }

    private static int Decode82(Span<byte> source, Span<short> target)
    {
        var index = 0;

        for (var group = 0; group < 18; group++)
        {
            for (var block = 0; block < 2; block++)
            {
                for (var sample = 0; sample < 28; sample++)
                {
                    for (var channel = 0; channel < 2; channel++)
                    {
                        var parameter = 4 + block * 2 + channel;
                        Globals.WriteLine(
                            $"{nameof(group)}: {group}, " +
                            $"{nameof(block)}: {block}, " +
                            $"{nameof(channel)}: {channel}, " +
                            $"{nameof(sample)}: {sample}, " +
                            $"{nameof(parameter)}: {parameter}, " +
                            $"");
                        var sp = source[parameter];
                        var sr = sp & 0xF;
                        var sf = (sp & 0x30) >> 4;
                        var f0 = Globals.PositiveFilters[sf];
                        var f1 = Globals.NegativeFilters[sf];

                        ref var older = ref History[channel][1];
                        ref var old = ref History[channel][0];

                        var k = 16 + block * 2 + sample * 4 + channel;
                        int t = source[k];
                        t = (t << 24) >> 24;
                        var s = (t << (8 - sr)) + (old * f0 + older * f1 + 32) / 64;
                        s = Math.Clamp(s, short.MinValue, short.MaxValue);
                        older = old;
                        old = s;
                        target[index++] = (short)s;
                    }
                }
            }

            source = source[128..];
        }

        return index;
    }
}