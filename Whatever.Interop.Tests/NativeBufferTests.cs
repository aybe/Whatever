namespace Whatever.Interop.Tests;

[TestClass]
public sealed class NativeBufferTests
{
    [TestMethod]
    public unsafe void TestAllocClearFree()
    {
        var allocator = NativeAllocator.Default;

        var alloc = allocator.Alloc<byte>(3);

        allocator.Clear(alloc, 3);

        Assert.AreEqual(0, alloc[0]);
        Assert.AreEqual(0, alloc[1]);
        Assert.AreEqual(0, alloc[2]);

        allocator.Free(alloc);
    }

    [TestMethod]
    public void TestBuffer1D()
    {
        using var buffer = new NativeBuffer1D<int>(3);

        ref var item0 = ref buffer[0];
        ref var item1 = ref buffer[1];
        ref var item2 = ref buffer[2];

        item0 = 1;
        item1 = 2;
        item2 = 3;

        Assert.AreEqual(1, item0);
        Assert.AreEqual(2, item1);
        Assert.AreEqual(3, item2);
    }

    [TestMethod]
    public void TestBuffer2D()
    {
        using var buffer = new NativeBuffer2D<int>(3, 3);

        ref var item00 = ref buffer[0, 0];
        ref var item01 = ref buffer[0, 1];
        ref var item02 = ref buffer[0, 2];

        ref var item10 = ref buffer[1, 0];
        ref var item11 = ref buffer[1, 1];
        ref var item12 = ref buffer[1, 2];

        ref var item20 = ref buffer[2, 0];
        ref var item21 = ref buffer[2, 1];
        ref var item22 = ref buffer[2, 2];

        item00 = 1;
        item01 = 2;
        item02 = 3;

        item10 = 4;
        item11 = 5;
        item12 = 6;

        item20 = 7;
        item21 = 8;
        item22 = 9;

        Assert.AreEqual(1, item00);
        Assert.AreEqual(2, item01);
        Assert.AreEqual(3, item02);

        Assert.AreEqual(4, item10);
        Assert.AreEqual(5, item11);
        Assert.AreEqual(6, item12);

        Assert.AreEqual(7, item20);
        Assert.AreEqual(8, item21);
        Assert.AreEqual(9, item22);

        var span0 = buffer[0];
        var span1 = buffer[1];
        var span2 = buffer[2];

        Assert.AreEqual(1, span0[0]);
        Assert.AreEqual(2, span0[1]);
        Assert.AreEqual(3, span0[2]);

        Assert.AreEqual(4, span1[0]);
        Assert.AreEqual(5, span1[1]);
        Assert.AreEqual(6, span1[2]);

        Assert.AreEqual(7, span2[0]);
        Assert.AreEqual(8, span2[1]);
        Assert.AreEqual(9, span2[2]);
    }
}