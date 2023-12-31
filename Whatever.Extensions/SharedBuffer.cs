﻿using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;

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
        public SharedBuffer(int length)
        {
            Array = Pool.Rent(Length = length);
        }

        /// <summary>
        ///     Gets the buffer length.
        /// </summary>
        public int Length { get; }

        /// <summary>
        ///     Gets the buffer as a <see cref="Memory{T}" />.
        /// </summary>
        public Memory<T> Memory => Array.AsMemory(0, Length);

        /// <summary>
        ///     Gets the buffer as a <see cref="Span{T}" />.
        /// </summary>
        public Span<T> Span => Array.AsSpan(0, Length);

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

        /// <summary>
        ///     Converts the value to a <see cref="Memory{T}" />.
        /// </summary>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
        public static implicit operator Memory<T>(SharedBuffer<T> buffer)
        {
            return buffer.Memory;
        }

        /// <summary>
        ///     Converts the value to a <see cref="ReadOnlyMemory{T}" />.
        /// </summary>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
        public static implicit operator ReadOnlyMemory<T>(SharedBuffer<T> buffer)
        {
            return buffer.Memory;
        }

        /// <summary>
        ///     Converts the value to a <see cref="Span{T}" />.
        /// </summary>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
        public static implicit operator Span<T>(SharedBuffer<T> buffer)
        {
            return buffer.Span;
        }

        /// <summary>
        ///     Converts the value to a <see cref="ReadOnlySpan{T}" />.
        /// </summary>
        [SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "<Pending>")]
        public static implicit operator ReadOnlySpan<T>(SharedBuffer<T> buffer)
        {
            return buffer.Span;
        }
    }
}