using System;
using System.Globalization;
using System.Text;

namespace Whatever.Extensions
{
    public sealed class TextProgressBar
    {
        private readonly object Lock = new();

        public StringBuilder Builder { get; } = new();

        public TextProgressBarOptions Options { get; set; } = new();

        /// <summary>
        ///     Clears the underlying <see cref="StringBuilder" />.
        /// </summary>
        public void Clear()
        {
            Builder.Clear();
        }

        /// <summary>
        ///     Updates the progress bar.
        /// </summary>
        /// <param name="value">
        ///     The progress value.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     <paramref name="value" /> is not between 0 and 1.
        /// </exception>
        public void Update(double value)
        {
            if (value is < 0.0d or > 1.0d)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be between 0 and 1.");
            }

            lock (Lock)
            {
                var width = Options.Width;

                var state = (int)Math.Floor(value * width);

                for (var j = 0; j < state; j++)
                {
                    Builder.Append(Options.Foreground);
                }

                for (var j = state; j < width; j++)
                {
                    Builder.Append(Options.Background);
                }

                if (Options.Text)
                {
                    Builder.Append($" {value.ToString($"P{Options.TextDigits}", CultureInfo.InvariantCulture)}");
                }
            }
        }

        public override string ToString()
        {
            lock (Lock)
            {
                return Builder.ToString();
            }
        }
    }
}