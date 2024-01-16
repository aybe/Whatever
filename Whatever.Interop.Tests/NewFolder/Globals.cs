namespace Whatever.Interop.Tests.NewFolder;

public static unsafe class Globals
{
    private const uint ChunksPerSector = 18;

    private const uint ChunkSize = 128;

    private const uint DataWordSize = sizeof(uint);

    private const uint DataWordsPerChunk = 28;

    private const uint ChunkMaxSamples = 2 * DataWordSize * DataWordsPerChunk * 2;

    public const uint SectorMaxSamples = ChunksPerSector * ChunkMaxSamples;

    private static readonly short[] PositiveFilters = [0, +60, +115, +98, +122];

    private static readonly short[] NegativeFilters = [0, 0, -52, -55, -60];

    public static bool Decode(Context ctx, SectorAudio output, Span<byte> sector)
    {
        DecodeNew(ctx, output, sector);
        return true;
        output.SampleCount = 0;

        var isStereo = (sector[19] & 0x3) != 0;
        var is8Bit = (sector[19] & 0x30) != 0;
        var sampleRate = (sector[19] & 0xC) != 0 ? 18900 : 37800;

        var samplesPerChunk = is8Bit
            ? ChunkMaxSamples / 4
            : ChunkMaxSamples / 2;

        fixed (byte* pDataIn1 = sector[(12 + 4 + 8)..])
        {
            var pDataIn = pDataIn1;

            fixed (short* pSamplesOut1 = output.Samples)
            {
                var pSamplesOut = pSamplesOut1;

                for (uint chunkIdx = 0; chunkIdx < ChunksPerSector; chunkIdx++)
                {
                    if (is8Bit)
                    {
                        if (isStereo)
                        {
                            Decode2(ctx, pDataIn, pSamplesOut, 4, 8, 0xFF, 8);
                        }
                        else
                        {
                            Decode1(ctx, pDataIn, pSamplesOut, 4, 8, 0xFF, 8);
                        }
                    }
                    else
                    {
                        if (isStereo)
                        {
                            Decode2(ctx, pDataIn, pSamplesOut, 8, 4, 0x0F, 12);
                        }
                        else
                        {
                            Decode1(ctx, pDataIn, pSamplesOut, 8, 4, 0x0F, 12);
                        }
                    }

                    pDataIn += ChunkSize;
                    pSamplesOut += samplesPerChunk;
                }
            }
        }

        output.Channels = (ushort)(isStereo ? 2 : 1);
        output.SampleCount = samplesPerChunk * ChunksPerSector;
        output.SampleRate = (uint)sampleRate;

        return true;
    }

    private static short DecodeAdpcmSample(
        in short unfilteredSample, ref short prevSample1, ref short prevSample2, in byte sampleShift, in short posFilter, in short negFilter)
    {
        var filteredSample = (unfilteredSample >> sampleShift) + (prevSample1 * posFilter + prevSample2 * negFilter + 32) / 64;

        var clamp = (short)Math.Clamp(filteredSample, short.MinValue, short.MaxValue);

        prevSample2 = prevSample1;
        prevSample1 = clamp;

        return clamp;
    }

    private static void Decode1(
        Context ctx, byte* pDataIn, in short* pSamplesOut, int blockCount, int blockScale, byte sampleMask, int sampleShift)
    {
        var words = (uint*)(pDataIn + 16);
        var samplesOut = pSamplesOut;

        for (var block = 0; block < blockCount; block++)
        {
            DecodeUnpack(pDataIn, block, out var shl, out var pos, out var neg);

            for (var sample = 0; sample < DataWordsPerChunk; sample++)
            {
                var unfilteredSample = GetSample(blockScale, sampleMask, sampleShift, words, sample, block);

                samplesOut[0] = DecodeAdpcmSample(unfilteredSample, ref ctx.History[0][0], ref ctx.History[0][1], shl, pos, neg);

                samplesOut += 1;
            }
        }
    }

    private static void Decode2(
        Context ctx, byte* pDataIn, in short* pSamplesOut, int blockCount, int blockScale, byte sampleMask, int sampleShift)
    {
        var words = (uint*)(pDataIn + 16);
        var samplesOut = pSamplesOut;

        for (var block = 0; block < blockCount; block++)
        {
            DecodeUnpack(pDataIn, block, out var shl, out var pos, out var neg);

            var channel = block & 1;

            var right = channel != 0;

            for (var sample = 0; sample < DataWordsPerChunk; sample++) // Note: the other channel will be handled by a separate block
            {
                var unfilteredSample = GetSample(blockScale, sampleMask, sampleShift, words, sample, block);

                samplesOut[0] = DecodeAdpcmSample(unfilteredSample, ref ctx.History[channel][0], ref ctx.History[channel][1], shl, pos, neg);

                samplesOut += 2;
            }

            if (right) // Put us onto the left channel for the next block
            {
                samplesOut -= 1;
            }
            else // Put us onto the right channel for the current block
            {
                samplesOut -= DataWordsPerChunk * 2;
                samplesOut += 1;
            }
        }
    }

    private static short GetSample(int blockScale, byte sampleMask, int sampleShift, uint* words, int sample, int block)
    {
        return (short)((byte)((words[sample] >> (block * blockScale)) & sampleMask) << sampleShift);
    }

    private static void DecodeUnpack(byte* pDataIn, int block, out byte shl, out short pos, out short neg)
    {
        var hdr = pDataIn[4 + block];
        var hdrShiftBits = (byte)(hdr & 0xFu);
        var hdrFilterBits = (byte)((hdr >> 4) & 0x3u);

        shl = (byte)(hdrShiftBits < 13 ? hdrShiftBits : 9);
        pos = PositiveFilters[hdrFilterBits];
        neg = NegativeFilters[hdrFilterBits];
    }

    #region temp

    private static int dst_left;
    private static readonly int dst_right = 1;
    private static int dst_mono;
    private static int old_left, old_right, old_mono;
    private static int older_left, older_right, older_mono;

    private static void DecodeNew(Context ctx, SectorAudio output, Span<byte> sector)
    {
        var isStereo = (sector[19] & 0x3) != 0;
        var is8Bit = (sector[19] & 0x30) != 0;
        var sampleRate = (sector[19] & 0xC) != 0 ? 18900 : 37800;
        var samplesPerChunk = is8Bit
            ? ChunkMaxSamples / 4
            : ChunkMaxSamples / 2;
        output.SampleRate = (uint)sampleRate;
        output.Channels = (ushort)(isStereo ? 2 : 1);
        output.SampleCount = samplesPerChunk * ChunksPerSector;
        var src = sector;
        src = src[(12 + 4 + 8)..];
        var dst0 = 0;
        var dst1 = 1;
        for (var i = 0; i < 18; i++)
        {
            for (var blk = 0; blk < 4; blk++)
            {
                if (isStereo)
                {
                    Decode28Nibbles(src, blk, 0, ref dst0, ref old_left, ref older_left, output.Samples, isStereo);
                    Decode28Nibbles(src, blk, 1, ref dst1, ref old_right, ref older_right, output.Samples, isStereo);
                }
                else
                {
                    Decode28Nibbles(src, blk, 0, ref dst0, ref old_mono, ref older_mono, output.Samples, isStereo);
                    Decode28Nibbles(src, blk, 1, ref dst0, ref old_mono, ref older_mono, output.Samples, isStereo);
                }
            }

            src = src[128..];
        }

        src = src[24..];
    }

    private static void Decode28Nibbles(
        Span<byte> src, int blk, int nibble, ref int dst, ref int old, ref int older, short[] outputSamples, bool isStereo)
    {
        var index = 4 + blk * 2 + nibble;
        var shift = 12 - (src[index] & 0xF);
        var filter = (src[index] & 0x30) >> 4;
        var f0 = PositiveFilters[filter];
        var f1 = NegativeFilters[filter];
        for (var j = 0; j < 28; j++)
        {
            var t = Signed4Bit((src[16 + blk + j * 4] >> (nibble * 4)) & 0x0F);
            var s = (t << shift) + (old * f0 + older * f1 + 32) / 64;
            s = Math.Clamp(s, short.MinValue, short.MaxValue);
            outputSamples[dst] = (short)s;
            dst += isStereo ? 2 : 1;
            older = old;
            old = s;
        }
    }

    private static int Signed4Bit(int i)
    {
        var j = (i << 28) >> 28;

        Console.WriteLine($"{i}, {j}");
        return j;
    }

    #endregion
}