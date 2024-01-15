using System.Runtime.InteropServices;

namespace Whatever.Interop.Tests.NewFolder;

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

        using var context = new Context();

        var output = new SectorAudio();

        var buffer = new byte[length];

        var samples = 0u;

        WriteWavHeader(target);

        while (source.Position < source.Length)
        {
            source.ReadExactly(buffer);

            var decode = Globals.Decode(context, output, buffer);

            Assert.IsTrue(decode); // TODO delete

            var span = output.Samples.AsSpan(0, (int)output.SampleCount);

            samples += output.SampleCount;

            target.Write(MemoryMarshal.AsBytes(span));
        }

        target.Position = 0;

        WriteWavHeader(target, 16, output.Channels, output.SampleRate, samples);

        var expectedHash = sourceFileName switch
        {
            "test_18900_4_1_2352.xa" => "dcd1bc2dba860d444ca178f194df0c111e8338a7f234e7335694ea8537deaa9e",
            "test_18900_4_2_2352.xa" => "577a3c4502edbef14beee087db9d4f8541275f77bb67484eb20d9adb68dacc1c",
            "test_18900_8_1_2352.xa" => "e48bf59a49b6bdbd5f7d023b84287a032ea93aad225821c45c37f730895494e3",
            "test_18900_8_2_2352.xa" => "23cbaf82a54f0d65079d35d5c54acb474ac8bd2de5166e796561b7f55cf56474",
            "test_37800_4_1_2352.xa" => "3388ee0f87068acf0acb4b23128ba560bfb3cfc12a86fe5ff1daaed489f5ac83",
            "test_37800_4_2_2352.xa" => "01bfa41e6892a8938494ec498bd2a676a02398e2b8c748f64265d345219ba48a",
            "test_37800_8_1_2352.xa" => "3183e6b960e4ac1055d7de500dbea4370f9da5117e424ea0dde04f01b5a863e5",
            "test_37800_8_2_2352.xa" => "92a8603174e5e36e2492a68557aa6d3a57243fa30b1e398fae81cf130fe22bbb",
            _ => throw new NotSupportedException()
        };

        var actualHash = target.GetSha256Hash();

        File.WriteAllBytes(targetFileName, target.ToArray());

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