using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        [SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification = "<Pending>")]
        public static T Instance { get; } = new();
    }
}