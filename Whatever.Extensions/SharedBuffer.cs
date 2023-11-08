using System;
using System.Buffers;

namespace Whatever.Extensions
{
    public readonly struct SharedBuffer<T> : IDisposable
    {
        private static ArrayPool<T> Pool { get; } = ArrayPool<T>.Shared;

        public readonly T[] Array;

        public SharedBuffer(int minimumLength)
        {
            Array = Pool.Rent(minimumLength);
        }

        public Memory<T> AsMemory(int start, int length)
        {
            return Array.AsMemory(start, length);
        }

        public Span<T> AsSpan(int start, int length)
        {
            return Array.AsSpan(start, length);
        }

        public void Dispose()
        {
            Pool.Return(Array);
        }

        public static implicit operator T[](SharedBuffer<T> buffer)
        {
            return buffer.Array;
        }

        public static implicit operator Memory<T>(SharedBuffer<T> buffer)
        {
            return buffer.Array;
        }

        public static implicit operator Span<T>(SharedBuffer<T> buffer)
        {
            return buffer.Array;
        }

        public static implicit operator ReadOnlyMemory<T>(SharedBuffer<T> buffer)
        {
            return buffer.Array;
        }

        public static implicit operator ReadOnlySpan<T>(SharedBuffer<T> buffer)
        {
            return buffer.Array;
        }
    }
}