using System.Diagnostics.CodeAnalysis;

namespace Whatever.Interop.Tests.NewFolder;

[SuppressMessage("ReSharper", "UnassignedReadonlyField")]
public readonly struct Sector : IEquatable<Sector>
{
    public readonly SectorSync Sync;
    public readonly SectorHeader Header;
    public readonly SectorSubHeader SubHeader1;
    public readonly SectorSubHeader SubHeader2;

    #region Equality members

    public bool Equals(Sector other)
    {
        return
            Sync.Equals(other.Sync) &&
            Header.Equals(other.Header) &&
            SubHeader1.Equals(other.SubHeader1) &&
            SubHeader2.Equals(other.SubHeader2);
    }

    public override bool Equals(object? obj)
    {
        return obj is Sector other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Sync, Header, SubHeader1, SubHeader2);
    }

    public static bool operator ==(Sector left, Sector right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Sector left, Sector right)
    {
        return !left.Equals(right);
    }

    #endregion
}