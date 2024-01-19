using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Base class for a singleton.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        /// <summary>
        ///     Gets the singleton instance for type <typeparamref name="T" />.
        /// </summary>
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
        public static T Instance { get; } = new();
    }
}