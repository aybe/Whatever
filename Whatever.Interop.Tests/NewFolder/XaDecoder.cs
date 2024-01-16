namespace Whatever.Interop.Tests.NewFolder;

public static class XaDecoder
{
    private static readonly int[][] History = [[0, 0], [0, 0]];

    public static void Decode(Span<byte> src, SectorAudio output)
    {
        var isStereo = (src[19] & 0x3) != 0;
        var is8Bit = (src[19] & 0x30) != 0;
        var sampleRate = (src[19] & 0xC) != 0 ? 18900 : 37800;
        var blocks = is8Bit ? 4 : 8;

        output.Channels = (ushort)(isStereo ? 2 : 1);
        output.SampleRate = (uint)sampleRate;

        Globals.WriteLine(
            $"{nameof(isStereo)}: {isStereo}, " +
            $"{nameof(is8Bit)}: {is8Bit}, " +
            $"{nameof(sampleRate)}: {sampleRate}, " +
            $"{nameof(blocks)}: {blocks}"
        );

        src = src[(12 + 4 + 8)..];

        if (is8Bit)
        {
            if (isStereo)
            {
                int i; // TODO
            }
            else
            {
                output.SampleCount = (uint)Decode81(src, output.Samples, blocks);
            }
        }
        else
        {
            if (isStereo)
            {
                int i; // TODO
            }
            else
            {
                int i; // TODO
            }
        }
    }

    private static int Decode81(Span<byte> source, Span<short> target, int blocks)
    {
        var index = 0;

        ref var older = ref History[0][1];
        ref var old = ref History[0][0];

        for (var group = 0; group < 18; group++)
        {
            Globals.WriteLine($"{nameof(group)}: {group}");

            for (var block = 0; block < blocks; block++)
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
                    int t = source[16 + block + sample * 4];
                    t = (t << 24) >> 24;
                    var s = (t << (8 - sr)) + (old * f0 + older * f1 + 32) / 64;
                    s = Math.Clamp(s, short.MinValue, short.MaxValue);
                    older = old;
                    old = s;
                    target[index++] = (short)s;
                }
            }

            source = source[128..];
        }

        return index;
    }
}