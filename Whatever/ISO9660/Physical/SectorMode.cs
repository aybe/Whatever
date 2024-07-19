namespace Whatever.ISO9660.Physical;

#pragma warning disable CA1028 // Enum Storage should be Int32
public enum SectorMode : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
{
    Mode0 = 0,
    Mode1 = 1,
    Mode2 = 2,
    Reserved = 3,
}