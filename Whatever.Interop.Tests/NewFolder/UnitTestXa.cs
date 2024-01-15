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

        using var context = new Context();

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
            "test_18900_4_1_2352.xa" => "28b0b2e4c6a233b0922a8cf6c6709a3d1a95cdb7b2a1c0d142c0f72e909ccf8e", // changed
            "test_18900_4_2_2352.xa" => "40b2d653915c6ef7d58c7de79f2f1f5749935c6603c605594b8992f32417868a",
            "test_18900_8_1_2352.xa" => "a48f1415cbdcb8444cd0d20226b39d5867e00658c15d9a3ab41dd17165c6aeae", // changed
            "test_18900_8_2_2352.xa" => "cdcf0f497528e006e1e4bdd17455439e3e9d6d0762c1a804253acd52064d9242",
            "test_37800_4_1_2352.xa" => "46f31730092481c3c07df60e4c059f2ccb183976b16d828e86af66ee332be10f", // changed
            "test_37800_4_2_2352.xa" => "53fde476d343409a2322c6def4f2d16eb7dd0bc08b0dc208f0451ba8222313b2",
            "test_37800_8_1_2352.xa" => "9e33db59c79cca78a9c638e8b7e5b8031a5d45b9f229e579de0fb076691800cb", // changed
            "test_37800_8_2_2352.xa" => "b1d0dcdff4daf5e23c615175559da61d3202e555826aa1bc8667e0ee48d6e1af",
            _ => throw new NotSupportedException()
        };

        var actualHash = target.GetSha256Hash();

        File.WriteAllBytes(targetFileName, target.ToArray());

        Assert.AreEqual(expectedHash, actualHash);
    }
}