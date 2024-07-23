using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Whatever.Tests;

[TestClass]
public class UnitTestXa : UnitTestBase
{
    public static IEnumerable<object[]> TestData =>
        Directory
            .EnumerateFiles(@"C:\Temp\PSX\XA", "test*.xa")
            .Select(s => new object[] { s });

    [DataTestMethod]
    [DynamicData(nameof(TestData))]
    public void Test(string path)
    {
        var sourceFileName = Path.GetFileName(path);
        var targetFileName = Path.ChangeExtension(path, ".wav");

        if (File.Exists(targetFileName))
        {
            File.Delete(targetFileName);
        }

        using var source = File.OpenRead(path);
        using var target = new MemoryStream();

        var offset = true switch
        {
            true when source.Length % 2336 == 0 => 0,
            true when source.Length % 2352 == 0 => 16,
            _ => throw new NotSupportedException()
        };

        var length = true switch
        {
            true when source.Length % 2336 == 0 => 2336,
            true when source.Length % 2352 == 0 => 2352,
            _ => throw new NotSupportedException()
        };

        Assert.IsTrue((offset is 0 && length is 2336) || (offset is 16 && length is 2352)); // TODO delete

        var ctx = new XaDecoderContext();

        var samples = 0;

        WriteWavHeader(target);

        var sector = new byte[2352];

        while (source.Position < source.Length)
        {
            source.ReadExactly(sector);

            var decode = ctx.Decode(sector);

            var outputSampleCount = ctx.Samples;

            samples += outputSampleCount;

            target.Write(MemoryMarshal.AsBytes(decode));
        }

        target.Position = 0;

        WriteWavHeader(target, 16, (ushort)ctx.Channels, (uint)ctx.Frequency, (uint)samples);

        var expectedHash = sourceFileName switch
        {
            "test_18900_4_1_2352.xa" => "dcd1bc2dba860d444ca178f194df0c111e8338a7f234e7335694ea8537deaa9e",
            "test_18900_4_2_2352.xa" => "6c63b04a0a84c2530188838f8b4e04dfea6fba50b2b478e5e2899c0cf8c2d37a",
            "test_18900_8_1_2352.xa" => "e48bf59a49b6bdbd5f7d023b84287a032ea93aad225821c45c37f730895494e3",
            "test_18900_8_2_2352.xa" => "751ad2f1333e52ebc00d09cf9f1fc13bc6def8d4644ad3c3b52f7fff6553d65d",
            "test_37800_4_1_2352.xa" => "3388ee0f87068acf0acb4b23128ba560bfb3cfc12a86fe5ff1daaed489f5ac83",
            "test_37800_4_2_2352.xa" => "42174760a35eb5349ea7c17e2ce177464577d3d2a4bfebd9d08e8788d782a095",
            "test_37800_8_1_2352.xa" => "3183e6b960e4ac1055d7de500dbea4370f9da5117e424ea0dde04f01b5a863e5",
            "test_37800_8_2_2352.xa" => "8785fb109d44b464cc339cef5aee4c392a482112172d345890d792bc29ddef08",
            _ => throw new NotSupportedException()
        };

        File.WriteAllBytes(targetFileName, target.ToArray());

        target.Position = 0;

        var actualHash = string.Concat(SHA256.HashData(target).Select(s => s.ToString("x2")));

        Assert.AreEqual(expectedHash, actualHash);
    }

    private static void WriteWavHeader(Stream stream, ushort bitsPerSample = 16, ushort channels = 2, uint sampleRate = 44100u, uint samples = 0)
    {
        const ushort formatTag = 1;

        var bytesPerSample = bitsPerSample / 8;

        var avgBytesPerSec = (uint)(sampleRate * bytesPerSample * channels);

        var blockAlign = (ushort)(bytesPerSample * channels);

        var writer = new BinaryWriter(stream);

        writer.Write("RIFF"u8.ToArray());
        writer.Write((uint)(40 + bytesPerSample * samples * channels));
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write(formatTag);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(avgBytesPerSec);
        writer.Write(blockAlign);
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(bytesPerSample * samples * channels);
    }
}