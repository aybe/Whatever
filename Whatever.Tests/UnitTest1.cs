using System.Diagnostics.CodeAnalysis;
using Whatever.Extensions;

namespace Whatever.Tests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public void TestStreamEndianness()
    {
        using var stream = new MemoryStream();

        var endianness = stream.GetEndianness();

        Assert.IsNull(endianness);

        var endianness1 = stream.SetEndianness(Endianness.BE);

        Assert.AreEqual(endianness, endianness1);

        using (stream.SetEndiannessScope(Endianness.LE))
        {
            var endianness2 = stream.GetEndianness();

            Assert.AreEqual(Endianness.LE, endianness2);
        }

        var endianness3 = stream.GetEndianness();

        Assert.AreEqual(Endianness.BE, endianness3);
    }

    [TestMethod]
    public void TestStreamPeek()
    {
        var pattern = new byte[] { 1, 2, 3, 4 };

        using var stream = new MemoryStream(pattern);

        var peek = stream.Peek(s => s.Read<int>(Endianness.BE));

        Assert.AreEqual(0x01020304, peek);

        Assert.AreEqual(0, stream.Position);

        var tryPeek1 = stream.TryPeek(s => s.Read<int>(Endianness.BE), out var result);

        Assert.IsTrue(tryPeek1);

        Assert.AreEqual(0x01020304, result);

        stream.Position = 4;

        var tryPeek2 = stream.TryPeek(s => s.Read<int>(Endianness.BE), out _);

        Assert.IsFalse(tryPeek2);
    }


    [TestMethod]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    [SuppressMessage("ReSharper", "InvokeAsExtensionMethod")]
    public async Task TestStreamRead()
    {
        var pattern = new byte[] { 1, 2, 3, 4 };

        using var stream = new MemoryStream(pattern);

        {
            stream.Position = 0;

            var read = StreamExtensions.Read<int>(stream, Endianness.BE);

            Assert.AreEqual(0x01020304, read);
        }

        {
            stream.Position = 0;

            var read = StreamExtensions.Read<int>(stream, Endianness.LE);

            Assert.AreEqual(0x04030201, read);
        }

        {
            stream.Position = 0;

            var read = await StreamExtensions.ReadAsync<int>(stream, Endianness.BE);

            Assert.AreEqual(0x01020304, read);
        }

        {
            stream.Position = 0;

            var read = await StreamExtensions.ReadAsync<int>(stream, Endianness.LE);

            Assert.AreEqual(0x04030201, read);
        }

#if NETSTANDARD2_1
        {
            stream.Position = 0;

            var buffer = new byte[4];

            StreamExtensions.ReadExactly(stream, buffer, 0, buffer.Length);

            CollectionAssert.AreEqual(pattern, buffer);
        }

        {
            stream.Position = 0;

            var buffer = new byte[4];

            StreamExtensions.ReadExactly(stream, buffer);

            CollectionAssert.AreEqual(pattern, buffer);
        }

        {
            stream.Position = 0;

            var buffer = new byte[4];

            await StreamExtensions.ReadExactlyAsync(stream, buffer, 0, buffer.Length);

            CollectionAssert.AreEqual(pattern, buffer);
        }

        {
            stream.Position = 0;

            var buffer = new byte[4];

            await StreamExtensions.ReadExactlyAsync(stream, buffer);

            CollectionAssert.AreEqual(pattern, buffer);
        }
#endif

        {
            stream.Position = 0;

            var value = stream.Read<TestEnum>();

            Assert.AreEqual(value, TestEnum.Value);
        }

        {
            stream.Position = 0;

            var value = await stream.ReadAsync<TestEnum>();

            Assert.AreEqual(value, TestEnum.Value);
        }
    }

    [TestMethod]
    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public async Task TestStreamStringAscii()
    {
        using var stream = new MemoryStream("ABCD"u8.ToArray());

        var ascii1 = stream.ReadStringAscii(4);

        Assert.AreEqual("ABCD", ascii1);

        stream.Position = 0;

        var ascii2 = await stream.ReadStringAsciiAsync(4);

        Assert.AreEqual("ABCD", ascii2);
    }

    private enum TestEnum : byte
    {
        Value = 1
    }
}