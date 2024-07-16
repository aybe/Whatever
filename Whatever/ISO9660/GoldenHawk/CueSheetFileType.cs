using System.Diagnostics.CodeAnalysis;

namespace Whatever.ISO9660.GoldenHawk;

[SuppressMessage("ReSharper", "IdentifierTypo")]
public enum CueSheetFileType
{
    Binary,
    Motorola,
    Audio,
    Aiff,
    Flac,
    Mp3,
    Wave
}