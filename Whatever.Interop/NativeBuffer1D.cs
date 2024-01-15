using System;

namespace Whatever.Interop
{
    public readonly unsafe struct NativeBuffer1D<T>
        : IDisposable
        where T : unmanaged
    {
        private readonly int Count;

        private readonly T* Items;

        public NativeBuffer1D(int count, NativeAllocator? allocator = null)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), count, null);
            }

            allocator ??= NativeAllocator.Default;

            var items = allocator.Alloc<T>(count.ToUInt32());

            NativeBuffer.Register(items, allocator);

            Count = count;
            Items = items;
        }

        public ref T this[int x] => ref Items[x];

        public Span<T> Span => new Span<T>(Items, Count);

        public void Dispose()
        {
            NativeBuffer.Dispose(Items);
        }
    }
}