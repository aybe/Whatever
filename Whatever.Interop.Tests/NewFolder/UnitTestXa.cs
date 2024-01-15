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
        var targetFileName = Path.ChangeExtension(path, ".raw");

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

        var context = new Context();

        var output = new SectorAudio();

        var buffer = new byte[length];

        while (source.Position < source.Length)
        {
            source.ReadExactly(buffer);

            var decode = Globals.Decode(context, output, buffer);

            Assert.IsTrue(decode); // TODO delete

            var span = output.Samples.AsSpan(0, (int)output.SampleCount);

            target.Write(MemoryMarshal.AsBytes(span));
        }

        var expectedHash = sourceFileName switch
        {
            "test_18900_4_1_2352.xa" => "d7b15a43ab752dda8d1dda562bd013c09840497efd1b3505f87d4096804b1943",
            "test_18900_4_2_2352.xa" => "40b2d653915c6ef7d58c7de79f2f1f5749935c6603c605594b8992f32417868a",
            "test_18900_8_1_2352.xa" => "cb307aca605ea00eb38e32f0ecd467ad6256823c3bb9d4b05aa599ed95bfb699",
            "test_18900_8_2_2352.xa" => "cdcf0f497528e006e1e4bdd17455439e3e9d6d0762c1a804253acd52064d9242",
            "test_37800_4_1_2352.xa" => "11a6698c0236df3159732c49a3e17ba453b2809787f42177766cdadc4ebebf3d",
            "test_37800_4_2_2352.xa" => "53fde476d343409a2322c6def4f2d16eb7dd0bc08b0dc208f0451ba8222313b2",
            "test_37800_8_1_2352.xa" => "60721b5e87459a9d77b33a5530b8c4a9a3624d991d11d9a600198369eebc6c54",
            "test_37800_8_2_2352.xa" => "b1d0dcdff4daf5e23c615175559da61d3202e555826aa1bc8667e0ee48d6e1af",
            _ => throw new NotSupportedException()
        };

        var actualHash = target.GetSha256Hash();

        File.WriteAllBytes(targetFileName, target.ToArray());

        Assert.AreEqual(expectedHash, actualHash);
    }
}