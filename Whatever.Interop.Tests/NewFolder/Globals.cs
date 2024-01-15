using System.Runtime.InteropServices;

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
        output.SampleCount = 0;

        var source = MemoryMarshal.Read<Sector>(sector);
        var isStereo = (source.SubHeader1.CodingInformation & 0x3) != 0;
        var is8Bit = ((source.SubHeader1.CodingInformation & 0x30) != 0 ? 8 : 4) >= 8;
        var is4Bit = is8Bit == false;
        var samplesPerChunk = GetSamplesPerAdpcmChunk(isStereo, is8Bit);

        fixed (byte* pDataIn1 = sector[(12 + 4 + 8)..])
        {
            var pDataIn = pDataIn1;

            fixed (short* pSamplesOut1 = output.Samples)
            {
                var pSamplesOut = pSamplesOut1;

                for (uint chunkIdx = 0; chunkIdx < ChunksPerSector; chunkIdx++)
                {
                    if (is4Bit)
                    {
                        if (isStereo)
                        {
                            Decode2(ctx, pDataIn, pSamplesOut, 8, 4, 0x0F, Sample4BitToSample16Bit);
                        }
                        else
                        {
                            Decode1(ctx, pDataIn, pSamplesOut, 8, 4, 0x0F, Sample4BitToSample16Bit);
                        }
                    }
                    else
                    {
                        if (isStereo)
                        {
                            Decode2(ctx, pDataIn, pSamplesOut, 4, 8, 0xFF, Sample8BitToSample16Bit);
                        }
                        else
                        {
                            Decode1(ctx, pDataIn, pSamplesOut, 4, 8, 0xFF, Sample8BitToSample16Bit);
                        }
                    }

                    pDataIn += ChunkSize;
                    pSamplesOut += samplesPerChunk;
                }
            }
        }

        output.SampleCount = samplesPerChunk * ChunksPerSector;
        output.SampleRate = (uint)((source.SubHeader1.CodingInformation & 0xC) != 0 ? 18900 : 37000);

        return true;
    }

    private static uint GetSamplesPerAdpcmChunk(in bool bStereo, in bool b8Bit)
    {
        if (bStereo) return b8Bit ? ChunkMaxSamples / 4 : ChunkMaxSamples / 2;

        return b8Bit ? ChunkMaxSamples / 2 : ChunkMaxSamples;
    }

    private static short Sample4BitToSample16Bit(in ushort nibble)
    {
        if ((nibble & 0x8) != 0) return (short)((nibble << 12) - 65536);

        return (short)(nibble << 12);
    }

    private static short Sample8BitToSample16Bit(in ushort @byte)
    {
        if ((@byte & 0x80) != 0) return (short)((@byte << 8) - 65536);

        return (short)(@byte << 8);
    }

    private static short DecodeAdpcmSample(
        in short unfilteredSample, ref short prevSample1, ref short prevSample2, in byte sampleShift,
        in short posFilter, in short negFilter)
    {
        var filteredSample = (unfilteredSample >> sampleShift) +
                             (prevSample1 * posFilter + prevSample2 * negFilter + 32) / 64;

        var clamp = (short)Math.Clamp(filteredSample, short.MinValue, short.MaxValue);

        prevSample2 = prevSample1;
        prevSample1 = clamp;

        return clamp;
    }

    private static void Decode1(
        Context ctx, byte* pDataIn, in short* pSamplesOut, int blockCount, int blockScale, byte sampleMask,
        SampleFunc sampleFunc)
    {
        var words = (uint*)(pDataIn + 16);
        var samplesOut = pSamplesOut;

        for (var block = 0; block < blockCount; block++)
        {
            DecodeUnpack(pDataIn, block, out var shl, out var pos, out var neg);

            for (var sample = 0;
                 sample < DataWordsPerChunk;
                 sample++) // Mono: double up the sample for the left and right channels
            {
                var unfilteredSample = sampleFunc((byte)((words[sample] >> (block * blockScale)) & sampleMask));

                samplesOut[0] = samplesOut[1] =
                    DecodeAdpcmSample(unfilteredSample, ref ctx.M1, ref ctx.M2, shl, pos, neg);

                samplesOut += 2;
            }
        }
    }

    private static void Decode2(
        Context ctx, byte* pDataIn, in short* pSamplesOut, int blockCount, int blockScale, byte sampleMask,
        SampleFunc sampleFunc)
    {
        var words = (uint*)(pDataIn + 16);
        var samplesOut = pSamplesOut;

        for (var block = 0; block < blockCount; block++)
        {
            DecodeUnpack(pDataIn, block, out var shl, out var pos, out var neg);

            var right = (block & 1) != 0;

            for (var sample = 0;
                 sample < DataWordsPerChunk;
                 sample++) // Note: the other channel will be handled by a separate block
            {
                var unfilteredSample = sampleFunc((byte)((words[sample] >> (block * blockScale)) & sampleMask));

                ref var last1 = ref right ? ref ctx.R1 : ref ctx.L1;
                ref var last2 = ref right ? ref ctx.R2 : ref ctx.L2;

                samplesOut[0] = DecodeAdpcmSample(unfilteredSample, ref last1, ref last2, shl, pos, neg);

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

    private static void DecodeUnpack(byte* pDataIn, int block, out byte shl, out short pos, out short neg)
    {
        var hdr = pDataIn[4 + block];
        var hdrShiftBits = (byte)(hdr & 0xFu);
        var hdrFilterBits = (byte)((hdr >> 4) & 0x3u);

        shl = (byte)(hdrShiftBits < 13 ? hdrShiftBits : 9);
        pos = PositiveFilters[hdrFilterBits];
        neg = NegativeFilters[hdrFilterBits];
    }

    private delegate short SampleFunc(in ushort value);
}