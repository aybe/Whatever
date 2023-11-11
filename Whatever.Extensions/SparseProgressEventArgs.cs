using System;

namespace Whatever.Extensions
{
    public sealed class SparseProgressEventArgs<T> : EventArgs
    {
        public SparseProgressEventArgs(T value)
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