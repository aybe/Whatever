using JetBrains.Annotations;

namespace Whatever.Interop.Tests.NewFolder;

public readonly struct SectorSync : IEquatable<SectorSync>
{
    public static SectorSync Valid { get; } =
        new(0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00);

    public SectorSync(
        byte byte01, byte byte02, byte byte03, byte byte04,
        byte byte05, byte byte06, byte byte07, byte byte08,
        byte byte09, byte byte10, byte byte11, byte byte12)
    {
        Byte01 = byte01;
        Byte02 = byte02;
        Byte03 = byte03;
        Byte04 = byte04;
        Byte05 = byte05;
        Byte06 = byte06;
        Byte07 = byte07;
        Byte08 = byte08;
        Byte09 = byte09;
        Byte10 = byte10;
        Byte11 = byte11;
        Byte12 = byte12;
    }

    [UsedImplicitly] public readonly byte
        Byte01,
        Byte02,
        Byte03,
        Byte04,
        Byte05,
        Byte06,
        Byte07,
        Byte08,
        Byte09,
        Byte10,
        Byte11,
        Byte12;

    #region Equality members

    public bool Equals(SectorSync other)
    {
        return
            Byte01 == other.Byte01 &&
            Byte02 == other.Byte02 &&
            Byte03 == other.Byte03 &&
            Byte04 == other.Byte04 &&
            Byte05 == other.Byte05 &&
            Byte06 == other.Byte06 &&
            Byte07 == other.Byte07 &&
            Byte08 == other.Byte08 &&
            Byte09 == other.Byte09 &&
            Byte10 == other.Byte10 &&
            Byte11 == other.Byte11 &&
            Byte12 == other.Byte12;
    }

    public override bool Equals(object? obj)
    {
        return obj is SectorSync other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(Byte01);
        hashCode.Add(Byte02);
        hashCode.Add(Byte03);
        hashCode.Add(Byte04);
        hashCode.Add(Byte05);
        hashCode.Add(Byte06);
        hashCode.Add(Byte07);
        hashCode.Add(Byte08);
        hashCode.Add(Byte09);
        hashCode.Add(Byte10);
        hashCode.Add(Byte11);
        hashCode.Add(Byte12);
        return hashCode.ToHashCode();
    }

    public static bool operator ==(SectorSync left, SectorSync right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SectorSync left, SectorSync right)
    {
        return !left.Equals(right);
    }

    #endregion
}