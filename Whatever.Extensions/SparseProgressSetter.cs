namespace Whatever.Extensions
{
    /// <summary>
    ///     Defines a method to set a progress change.
    /// </summary>
    public delegate void SparseProgressSetter<T>(ref T source, double value);
}