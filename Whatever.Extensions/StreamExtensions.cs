using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="Stream" />.
    /// </summary>
    public static class StreamExtensions
    {
        #region Endianness

        /// <summary>
        ///     Gets the endianness of current environment.
        /// </summary>
        [PublicAPI]
        public static Endianness EnvironmentEndianness { get; } = BitConverter.IsLittleEndian ? Endianness.LE : Endianness.BE;

        private static ConcurrentDictionary<Stream, Endianness?> EndiannessDictionary { get; } = new();

        /// <summary>
        ///     Gets the endianness for this instance.
        /// </summary>
        /// <param name="stream">
        ///     The source stream.
        /// </param>
        /// <returns>
        ///     The endianness or <c>null</c> if none is explicitly set.
        /// </returns>
        public static Endianness? GetEndianness(this Stream stream)
        {
            var endianness = EndiannessDictionary.GetOrAdd(stream, default(Endianness?));

            return endianness;
        }

        /// <summary>
        ///     Sets the endianness for this instance.
        /// </summary>
        /// <param name="stream">
        ///     The source stream.
        /// </param>
        /// <param name="endianness">
        ///     The endianness, or <c>null</c> to follow <see cref="EnvironmentEndianness" />.
        /// </param>
        /// <returns>
        ///     The previous endianness that was set.
        /// </returns>
        public static Endianness? SetEndianness(this Stream stream, Endianness? endianness)
        {
            var previous = stream.GetEndianness();

            EndiannessDictionary[stream] = endianness;

            return previous;
        }

        /// <summary>
        ///     Gets a disposable object to temporarily switch this instance to specified endianness.
        /// </summary>
        /// <param name="stream">
        ///     The source stream.
        /// </param>
        /// <param name="endianness">
        ///     The endianness, or <c>null</c> to follow <see cref="EnvironmentEndianness" />.
        /// </param>
        /// <returns>
        ///     A disposable object to use with <c>using</c> keyword.
        /// </returns>
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

        /// <summary>
        ///     Peeks an object using a function.
        /// </summary>
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

        /// <summary>
        ///     Tries to peek an object using a function.
        /// </summary>
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

        private static int SizeOf<T>() where T : unmanaged
        {
            var type = typeof(T);

            if (type.IsEnum)
            {
                type = type.GetEnumUnderlyingType();
            }

            var sizeOf = Marshal.SizeOf(type);

            return sizeOf;
        }

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

        /// <summary>
        ///     Reads an unmanaged type with specified endianness.
        /// </summary>
        public static T Read<T>(
            this Stream stream, Endianness? endianness = null)
            where T : unmanaged
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var length = SizeOf<T>();

            using var buffer = new SharedBuffer<byte>(length);

            var span = buffer.Span;

            stream.ReadExactly(span);

            TryReverseEndianness(endianness ?? stream.GetEndianness(), span);

            var value = MemoryMarshal.Read<T>(span);

            return value;
        }

        /// <summary>
        ///     See <see cref="Read{T}" />.
        /// </summary>
        public static async Task<T> ReadAsync<T>(
            this Stream stream, Endianness? endianness = null, CancellationToken cancellationToken = default)
            where T : unmanaged
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            var length = SizeOf<T>();

            using var buffer = new SharedBuffer<byte>(length);

            await stream
                .ReadExactlyAsync(buffer.Memory, cancellationToken)
                .ConfigureAwait(false);

            TryReverseEndianness(endianness ?? stream.GetEndianness(), buffer.Span);

            var value = MemoryMarshal.Read<T>(buffer.Span);

            return value;
        }

        /// <summary>
        ///     See <see cref="ReadExactly(System.IO.Stream,Span{byte})" />.
        /// </summary>
        public static byte[] ReadExactly(this Stream stream, int count)
        {
            var buffer = new byte[count];

            stream.ReadExactly(buffer);

            return buffer;
        }

        /// <summary>
        ///     See <see cref="ReadExactly(System.IO.Stream,Span{byte})" />.
        /// </summary>
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

        /// <summary>
        ///     Reads an exact number of bytes.
        /// </summary>
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

        /// <summary>
        ///     See <see cref="ReadExactly(System.IO.Stream,Span{byte})" />.
        /// </summary>
        public static async Task<byte[]> ReadExactlyAsync(
            this Stream stream, int count, CancellationToken cancellationToken = default)
        {
            var buffer = new byte[count];

            await stream.ReadExactlyAsync(buffer, cancellationToken).ConfigureAwait(false);

            return buffer;
        }

        /// <summary>
        ///     See <see cref="ReadExactly(System.IO.Stream,Span{byte})" />.
        /// </summary>
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

        /// <summary>
        ///     See <see cref="ReadExactly(System.IO.Stream,Span{byte})" />.
        /// </summary>
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

        /// <summary>
        ///     Reads an ASCII string.
        /// </summary>
        public static string ReadStringAscii(this Stream stream, int length)
        {
            Span<byte> buffer = stackalloc byte[length];

            stream.ReadExactly(buffer);

            var ascii = Encoding.ASCII.GetString(buffer);

            return ascii;
        }

        /// <summary>
        ///     See <see cref="ReadStringAscii" />.
        /// </summary>
        public static async Task<string> ReadStringAsciiAsync(this Stream stream, int length)
        {
            using var buffer = new SharedBuffer<byte>(length);

            await stream.ReadExactlyAsync(buffer.Memory).ConfigureAwait(false);

            var ascii = Encoding.ASCII.GetString(buffer.Span);

            return ascii;
        }

        #endregion
    }
}