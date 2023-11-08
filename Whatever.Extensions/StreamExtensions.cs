using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Whatever.Extensions
{
    public static class StreamExtensions
    {
        #region Endianness

        [PublicAPI] public static Endianness EnvironmentEndianness { get; } = BitConverter.IsLittleEndian ? Endianness.LE : Endianness.BE;

        private static ConcurrentDictionary<Stream, Endianness?> EndiannessDictionary { get; } = new();

        public static Endianness? GetEndianness(this Stream stream)
        {
            var endianness = EndiannessDictionary.GetOrAdd(stream, default(Endianness?));

            return endianness;
        }

        public static Endianness? SetEndianness(this Stream stream, Endianness? endianness)
        {
            var previous = stream.GetEndianness();

            EndiannessDictionary[stream] = endianness;

            return previous;
        }

        public static IDisposable SetEndiannessScope(this Stream stream, Endianness? endianness)
        {
            var scope = new EndiannessScope(stream, endianness);

            return scope;
        }

        private readonly struct EndiannessScope : IDisposable
        {
            private readonly Stream Stream;

            private readonly Endianness? Endianness;

            public EndiannessScope(Stream stream, Endianness? endianness = null)
            {
                Endianness = (Stream = stream).SetEndianness(endianness);
            }

            void IDisposable.Dispose()
            {
                Stream.SetEndianness(Endianness);
            }
        }

        #endregion

        #region Peek

        public static T Peek<T>(this Stream stream, Func<Stream, T> reader)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            var position = stream.Position;

            var value = reader(stream);

            stream.Position = position;

            return value;
        }

        public static bool TryPeek<T>(this Stream stream, Func<Stream, T> reader, out T result)
        {
            try
            {
                result = stream.Peek(reader);

                return true;
            }
            catch (EndOfStreamException)
            {
                result = default!;

                return false;
            }
        }

        #endregion

        #region Read

        private static void ThrowIfNotEqual<TValue>(TValue value1, TValue value2, Func<Exception> exception)
        {
            if (!EqualityComparer<TValue>.Default.Equals(value1, value2))
            {
                throw exception();
            }
        }

        private static void TryReverseEndianness(Endianness? endianness, Span<byte> buffer)
        {
            var actual = EnvironmentEndianness;

            if ((endianness ?? actual) != actual)
            {
                buffer.Reverse();
            }
        }

        public static T Read<T>(
            this Stream stream, Endianness? endianness = null)
            where T : unmanaged
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var length = Marshal.SizeOf<T>();

            using var buffer = new SharedBuffer<byte>(length);

            var span = buffer.AsSpan(0, length);

            stream.ReadExactly(span);

            TryReverseEndianness(endianness ?? stream.GetEndianness(), span);

            var value = MemoryMarshal.Read<T>(span);

            return value;
        }

        public static async Task<T> ReadAsync<T>(
            this Stream stream, Endianness? endianness = null, CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var length = Marshal.SizeOf<T>();

            using var buffer = new SharedBuffer<byte>(length);

            var memory = buffer.AsMemory(0, length);

            await stream
                .ReadExactlyAsync(memory, cancellationToken)
                .ConfigureAwait(false);

            TryReverseEndianness(endianness ?? stream.GetEndianness(), memory.Span);

            var value = MemoryMarshal.Read<T>(buffer.AsReadOnlySpan());

            return value;
        }

        public static void ReadExactly(
            this Stream stream, byte[] buffer, int offset, int count)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var read = stream.Read(buffer, offset, count);

            ThrowIfNotEqual(read, count, () => new EndOfStreamException());
        }

        public static void ReadExactly(
            this Stream stream, Span<byte> buffer)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var read = stream.Read(buffer);

            ThrowIfNotEqual(read, buffer.Length, () => new EndOfStreamException());
        }

        public static async Task ReadExactlyAsync(
            this Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var read = await stream
                .ReadAsync(buffer.AsMemory(offset, count), cancellationToken)
                .ConfigureAwait(false);

            ThrowIfNotEqual(read, count, () => new EndOfStreamException());
        }

        public static async ValueTask ReadExactlyAsync(
            this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var read = await stream
                .ReadAsync(buffer, cancellationToken)
                .ConfigureAwait(false);

            ThrowIfNotEqual(read, buffer.Length, () => new EndOfStreamException());
        }

        #endregion
    }
}