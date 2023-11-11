using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Whatever.Extensions
{
    /// <summary>
    ///     <see cref="IProgress{T}" /> that reports progress changes sparingly.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of progress update value.
    /// </typeparam>
    public sealed class SparseProgress<T> : IProgress<T>
        // - we don't want to force using a base class nor implementing an interface
        // - we want to allow the use of structs in addition to using classes
        // - we must reflect snapped value else callback would get wrong progress
    {
        private readonly Action<T>? Action;

        private readonly SynchronizationContext Context = SynchronizationContext.Current ?? new SynchronizationContext();

        private readonly SparseProgressGetter<T> Getter;

        private readonly SparseProgressSetter<T> Setter;

        private double Value;

        public SparseProgress(SparseProgressGetter<T> getter, SparseProgressSetter<T> setter, Action<T>? action = null)
        {
            Getter = getter;
            Setter = setter;
            Action = action;
        }

        /// <summary>
        ///     Gets or sets report granularity, e.g. 0 for 1% steps, 1 for 0.1% steps, etc, between 0 and 15.
        /// </summary>
        public int Digits { get; init; }

        /// <summary>
        ///     Whether handlers should be invoked synchronously.
        /// </summary>
        public bool Synchronous { get; set; }

        public void Report(T value)
        {
            var input = Getter(ref value);

            var step1 = Math.Round(input * 100.0d, Digits) / 100.0d;
            var step2 = Math.Round(Value * 100.0d, Digits) / 100.0d;

            Value = input;

            var compare = step1.CompareTo(step2);

            if (compare <= 0)
            {
                return;
            }

            Setter(ref value, step1);

            if (Action == null && ProgressChanged == null)
            {
                return;
            }

            if (Synchronous)
            {
                Context.Send(Callback, value);
            }
            else
            {
                Context.Post(Callback, value);
            }
        }

        /// <summary>
        ///     Updates a progress update value to match progress report granularity.
        /// </summary>
        /// <param name="value">
        ///     The value of the updated progress.
        /// </param>
        /// <param name="index">
        ///     The zero-based index of the current iteration.
        /// </param>
        /// <param name="count">
        ///     The total number of iterations.
        /// </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="value" /> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="index" /> is less than zero or greater or equal than <paramref name="count" />.
        /// </exception>
        public void Update([DisallowNull] ref T value, int index, int count)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (index < 0 || index > count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            var percentage = (index + 1) / (double)count;

            Setter(ref value, percentage);
        }

        private void Callback(object? state)
        {
            if (state is not T value)
            {
                throw new ArgumentNullException(nameof(state));
            }

            Action?.Invoke(value);

            ProgressChanged?.Invoke(this, new SparseProgressEventArgs<T>(value));
        }

        public event EventHandler<SparseProgressEventArgs<T>>? ProgressChanged;
    }
}