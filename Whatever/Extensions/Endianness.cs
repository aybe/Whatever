using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions;

/// <summary>
///     Specifies the endianness to use for endian-aware methods.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Endianness
{
    /// <summary>
    ///     MSB, e.g. Motorola.
    /// </summary>
    BE = 0,

    /// <summary>
    ///     LSB, e.g. Intel.
    /// </summary>
    LE = 1
}