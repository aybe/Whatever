using System.Buffers;
using JetBrains.Annotations;

namespace Whatever.Extensions;

/// <summary>
///     Disposable scope for <see cref="ArrayPool{T}" />.
/// </summary>
public readonly struct ArrayPoolScope<T> : IDisposable, IEquatable<ArrayPoolScope<T>>
{
    private readonly T[] Array;

    private readonly int Length;

    private readonly ArrayPool<T> Pool;

    [UsedImplicitly]
    public ArrayPoolScope()
    {
        throw new InvalidOperationException("Use parameterized constructor.");
    }

    public ArrayPoolScope(int length, ArrayPool<T>? pool = null)
    {
        Array = (Pool = pool ?? ArrayPool<T>.Shared).Rent(Length = length);

        Span.Clear();
    }

    public Memory<T> Memory => Array.AsMemory(0, Length);

    public Span<T> Span => Array.AsSpan(0, Length);

    public void Dispose()
    {
        Pool.Return(Array);
    }

    public bool Equals(ArrayPoolScope<T> other)
    {
        return Array.Equals(other.Array);
    }

    public override bool Equals(object? obj)
    {
        return obj is ArrayPoolScope<T> other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Array.GetHashCode();
    }

    public static bool operator ==(ArrayPoolScope<T> left, ArrayPoolScope<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArrayPoolScope<T> left, ArrayPoolScope<T> right)
    {
        return !left.Equals(right);
    }

    public static implicit operator Memory<T>(ArrayPoolScope<T> scope)
    {
        return scope.Memory;
    }

    public static implicit operator ReadOnlyMemory<T>(ArrayPoolScope<T> scope)
    {
        return scope.Memory;
    }

    public static implicit operator Span<T>(ArrayPoolScope<T> scope)
    {
        return scope.Span;
    }

    public static implicit operator ReadOnlySpan<T>(ArrayPoolScope<T> scope)
    {
        return scope.Span;
    }
}