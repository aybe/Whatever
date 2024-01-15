using System.Diagnostics.CodeAnalysis;

namespace Whatever.Interop.Tests.NewFolder;

[SuppressMessage("ReSharper", "UnassignedReadonlyField")]
public readonly struct SectorHeader : IEquatable<SectorHeader>
{
    public readonly byte Minutes;
    public readonly byte Seconds;
    public readonly byte Sectors;
    public readonly SectorMode Mode;

    public override string ToString()
    {
        return $"{Minutes:X2}:{Seconds:X2}.{Sectors:X2}, {Mode}";
    }

    #region Equality members

    public bool Equals(SectorHeader other)
    {
        return
            Minutes == other.Minutes &&
            Seconds == other.Seconds &&
            Sectors == other.Sectors &&
            Mode == other.Mode;
    }

    public override bool Equals(object? obj)
    {
        return obj is SectorHeader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Minutes, Seconds, Sectors, (int)Mode);
    }

    public static bool operator ==(SectorHeader left, SectorHeader right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SectorHeader left, SectorHeader right)
    {
        return !left.Equals(right);
    }

    #endregion
}