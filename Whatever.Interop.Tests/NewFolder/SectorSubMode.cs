namespace Whatever.Interop.Tests.NewFolder;

[Flags]
public enum SectorSubMode : byte
{
    None = 0,
    EndOfRecord = 1 << 0,
    Video = 1 << 1,
    Audio = 1 << 2,
    Data = 1 << 3,
    Trigger = 1 << 4,
    Form = 1 << 5,
    RealTimeSector = 1 << 6,
    EndOfFile = 1 << 7
}