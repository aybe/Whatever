namespace Whatever.Extensions
{
    /// <summary>
    ///     Options for <see cref="TextProgressBar" />.
    /// </summary>
    public sealed class TextProgressBarOptions
    {
        /// <summary>
        ///     Gets or sets the character used to draw the progress bar background.
        /// </summary>
        public char Background { get; set; } = '░';

        /// <summary>
        ///     Gets or sets the character used to draw the progress bar foreground.
        /// </summary>
        public char Foreground { get; set; } = '█';

        /// <summary>
        ///     Gets or sets whether to show text percentage at right of progress bar.
        /// </summary>
        public bool Text { get; set; } = true;

        /// <summary>
        ///     Gets or sets the number of fractional digits for the text percentage.
        /// </summary>
        public uint TextDigits { get; set; } = 2;

        /// <summary>
        ///     Gets or sets the width in characters for the progress bar.
        /// </summary>
        public uint Width { get; set; } = 20;
    }
}