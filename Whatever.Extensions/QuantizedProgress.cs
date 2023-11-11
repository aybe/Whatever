using System;
using System.Globalization;
using System.Threading;

namespace Whatever.Extensions
{
    /// <summary>
    ///     <see cref="IProgress{T}" /> that only reports after a significant change in progress.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of progress update value.
    /// </typeparam>
    public sealed class QuantizedProgress<T> : IProgress<T>
    {
        private readonly SendOrPostCallback Callback;

        private readonly SynchronizationContext Context = SynchronizationContext.Current ?? new SynchronizationContext();

        private readonly Action<T>? Handler;

        private T Value = default!;

        public QuantizedProgress()
        {
            Callback = SendOrPostCallback;
        }

        public QuantizedProgress(Action<T>? handler)
            : this()
        {
            Handler = handler;
        }

        /// <summary>
        ///     Function to convert a <typeparamref name="T" /> value to a <see cref="double" /> value between 0 and 1.
        /// </summary>
        public Func<T, double> Converter { get; set; } = s => Convert.ToDouble(s, CultureInfo.InvariantCulture);

        /// <summary>
        ///     Number of fractional digits to quantize a progress value.
        /// </summary>
        public int Digits { get; set; } = 3;

        /// <summary>
        ///     Whether handlers should be invoked synchronously.
        /// </summary>
        public bool Synchronous { get; set; }

        public void Report(T value)
        {
            var percent1 = ToPercent(value);
            var percent2 = ToPercent(Value);

            if (percent1 <= percent2)
            {
                return;
            }

            Value = value;

            if (Handler == null && ProgressChanged == null)
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

        public event EventHandler<QuantizedProgressEventArgs<T>>? ProgressChanged;

        private void SendOrPostCallback(object? state)
        {
            var value = (T)state!;

            Handler?.Invoke(value);

            ProgressChanged?.Invoke(this, new QuantizedProgressEventArgs<T>(value));
        }

        private int ToPercent(T value)
        {
            var input = Converter(value);

            if (input is < 0.0d or > 1.0d)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }

            var round = Math.Round(input, Digits);

            var floor = Math.Floor(round * 100.0d);

            return (int)floor;
        }
    }
}