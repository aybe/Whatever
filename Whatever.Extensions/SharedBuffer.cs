using System;
using System.Buffers;

namespace Whatever.Extensions
{
    public readonly struct SharedBuffer<T> : IDisposable, IEquatable<SharedBuffer<T>>
    {
        private static ArrayPool<T> Pool { get; } = ArrayPool<T>.Shared;

        private readonly T[] Array;

        public SharedBuffer(int minimumLength)
        {
            Array = Pool.Rent(minimumLength);
        }

        public T[] AsArray()
        {
            return Array;
        }

        public Memory<T> AsMemory()
        {
            return new Memory<T>(Array);
        }

        public Memory<T> AsMemory(int start, int length)
        {
            return new Memory<T>(Array, start, length);
        }

        public Span<T> AsSpan()
        {
            return new Span<T>(Array);
        }

        public Span<T> AsSpan(int start, int length)
        {
            return new Span<T>(Array, start, length);
        }

        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            return new ReadOnlyMemory<T>(Array);
        }

        public ReadOnlyMemory<T> AsReadOnlyMemory(int start, int length)
        {
            return new ReadOnlyMemory<T>(Array, start, length);
        }

        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return new ReadOnlySpan<T>(Array);
        }

        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
            return new ReadOnlySpan<T>(Array, start, length);
        }

        public void Dispose()
        {
            Pool.Return(Array);
        }

        public bool Equals(SharedBuffer<T> other)
        {
            return Array.Equals(other.Array);
        }

        public override bool Equals(object? obj)
        {
            return obj is SharedBuffer<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Array.GetHashCode();
        }

        public static bool operator ==(SharedBuffer<T> left, SharedBuffer<T> right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SharedBuffer<T> left, SharedBuffer<T> right)
        {
            return !left.Equals(right);
        }
    }
}