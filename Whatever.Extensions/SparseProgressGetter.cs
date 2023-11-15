namespace Whatever.Extensions
{
    /// <summary>
    ///     Defines a method to get a progress change.
    /// </summary>
    public delegate double SparseProgressGetter<T>(ref T source);
}