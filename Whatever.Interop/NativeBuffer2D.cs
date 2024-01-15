using System;

namespace Whatever.Interop
{
    public readonly unsafe struct NativeBuffer2D<T>
        : IDisposable
        where T : unmanaged
    {
        private readonly (int X, int Y) Count;

        private readonly T** Items;

        public NativeBuffer2D(int ySize, int xSize, NativeAllocator? allocator = null)
        {
            if (ySize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(ySize), ySize, null);
            }

            if (xSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(xSize), xSize, null);
            }

            allocator ??= NativeAllocator.Default;

            var items = (T**)allocator.Alloc<IntPtr>(ySize);

            NativeBuffer.Register(items, allocator);

            for (var i = 0; i < xSize; i++)
            {
                var item = allocator.Alloc<T>(xSize);

                NativeBuffer.Register(item, allocator);

                items[i] = item;
            }

            Count = (xSize, ySize);
            Items = items;
        }

        public ref T this[int y, int x]
        {
            get
            {
                if (y < 0 || y >= Count.Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(y), y, null);
                }

                if (x < 0 || x >= Count.X)
                {
                    throw new ArgumentOutOfRangeException(nameof(x), x, null);
                }

                ref var item = ref Items[y][x];

                return ref item;
            }
        }

        public Span<T> this[int y]
        {
            get
            {
                if (y < 0 || y >= Count.Y)
                {
                    throw new ArgumentOutOfRangeException(nameof(y), y, null);
                }

                var item = Items[y];

                var span = new Span<T>(item, Count.X);

                return span;
            }
        }

        public void Dispose()
        {
            for (var i = 0; i < Count.Y; i++)
            {
                var item = Items[i];

                NativeBuffer.Dispose(item);
            }

            NativeBuffer.Dispose(Items);
        }
    }
}