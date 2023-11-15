using System;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Event args for <see cref="SparseProgress{T}" />.
    /// </summary>
    public sealed class SparseProgressEventArgs<T> : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of <see cref="SparseProgressEventArgs{T}" />.
        /// </summary>
        /// <param name="value">
        ///     The progress value.
        /// </param>
        public SparseProgressEventArgs(T value)
        {
            Value = value;
        }

        /// <summary>
        ///     Gets the progress value for this instance.
        /// </summary>
        public T Value { get; init; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}