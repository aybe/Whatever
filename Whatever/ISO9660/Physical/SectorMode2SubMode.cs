namespace Whatever.ISO9660.Physical;

[Flags]
#pragma warning disable CA1028 // Enum Storage should be Int32
public enum SectorMode2SubMode : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
{
    EndOfRecord = 1 << 0,
    VideoBlock = 1 << 1,
    AudioBlock = 1 << 2,
    DataBlock = 1 << 3,
    TriggerBlock = 1 << 4,
    Form = 1 << 5,
    RealTimeBlock = 1 << 6,
    EndOfBlock = 1 << 7,
}