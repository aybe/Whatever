namespace Whatever.Extensions
{
    public sealed class TextProgressBarOptions
    {
        public char Background { get; set; } = '░';

        public char Foreground { get; set; } = '█';

        public bool Text { get; set; } = true;

        public uint TextDigits { get; set; } = 2;

        public uint Width { get; set; } = 20;
    }
}