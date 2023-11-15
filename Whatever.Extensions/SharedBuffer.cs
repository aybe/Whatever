using System;
using System.Buffers;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Shared buffer, an alternative to lack of <see cref="Span{T}" /> in async scenarios.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of elements in buffer.
    /// </typeparam>
    public readonly struct SharedBuffer<T> : IDisposable, IEquatable<SharedBuffer<T>>
    {
        private static ArrayPool<T> Pool { get; } = ArrayPool<T>.Shared;

        private readonly T[] Array;

        /// <summary>
        ///     See <see cref="ArrayPool{T}.Rent" />.
        /// </summary>
        public SharedBuffer(int minimumLength)
        {
            Array = Pool.Rent(minimumLength);
        }

        /// <summary>
        ///     Gets the underlying buffer as an array.
        /// </summary>
        public T[] AsArray()
        {
            return Array;
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="Memory{T}" />.
        /// </summary>
        public Memory<T> AsMemory()
        {
            return new Memory<T>(Array);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="Memory{T}" />.
        /// </summary>
        public Memory<T> AsMemory(int start, int length)
        {
            return new Memory<T>(Array, start, length);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="Span{T}" />.
        /// </summary>
        public Span<T> AsSpan()
        {
            return new Span<T>(Array);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="Span{T}" />.
        /// </summary>
        public Span<T> AsSpan(int start, int length)
        {
            return new Span<T>(Array, start, length);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="ReadOnlyMemory{T}" />.
        /// </summary>
        public ReadOnlyMemory<T> AsReadOnlyMemory()
        {
            return new ReadOnlyMemory<T>(Array);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="ReadOnlyMemory{T}" />.
        /// </summary>
        public ReadOnlyMemory<T> AsReadOnlyMemory(int start, int length)
        {
            return new ReadOnlyMemory<T>(Array, start, length);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="ReadOnlySpan{T}" />.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan()
        {
            return new ReadOnlySpan<T>(Array);
        }

        /// <summary>
        ///     Gets the underlying buffer as a <see cref="ReadOnlySpan{T}" />.
        /// </summary>
        public ReadOnlySpan<T> AsReadOnlySpan(int start, int length)
        {
            return new ReadOnlySpan<T>(Array, start, length);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Pool.Return(Array);
        }

        /// <inheritdoc />
        public bool Equals(SharedBuffer<T> other)
        {
            return Array.Equals(other.Array);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is SharedBuffer<T> other && Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Array.GetHashCode();
        }

        /// <summary>
        ///     Compares two instances for equality.
        /// </summary>
        public static bool operator ==(SharedBuffer<T> left, SharedBuffer<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Compares two instances for inequality.
        /// </summary>
        public static bool operator !=(SharedBuffer<T> left, SharedBuffer<T> right)
        {
            return !left.Equals(right);
        }
    }
}