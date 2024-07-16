using Whatever.ISO9660.Logical;
using Whatever.ISO9660.Physical;
using Whatever.Tests.ISO9660.Templates;

namespace Whatever.Tests.ISO9660;

[TestClass]
public sealed class UnitTestIsoFindFile : UnitTestBase
{
    public static IEnumerable<object[]> TestIsoFindFileInit()
    {
        var files = TestData.GetJsonTestData<TestDataIsoFindFile>(@"Templates\TestDataIsoFindFile.json");

        foreach (var file in files)
        {
            yield return new object[] { file.Source, file.Target, file.Exists };
        }
    }

    [TestMethod]
    [DynamicData(nameof(TestIsoFindFileInit), DynamicDataSourceType.Method)]
    public async Task TestIsoFindFile(string source, string target, bool exists)
    {
        await using var disc = Disc.Open(source);

        var ifs = IsoFileSystem.Read(disc);

        var tryFindFile = ifs.TryFindFile(target, out var result);

        Assert.AreEqual(exists, tryFindFile);

        if (result == null)
        {
            return;
        }

        WriteLine(() => result);
    }
}