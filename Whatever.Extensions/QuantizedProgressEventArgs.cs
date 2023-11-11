using System;

namespace Whatever.Extensions
{
    public sealed class QuantizedProgressEventArgs<T> : EventArgs
    {
        public QuantizedProgressEventArgs(T value)
        {
            Value = value;
        }

        public T Value { get; set; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}