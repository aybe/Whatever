using System.Diagnostics.CodeAnalysis;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Specifies the endianness.
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum Endianness
    {
        /// <summary>
        ///     Big-endian, i.e. MSB.
        /// </summary>
        BE = 0,

        /// <summary>
        ///     Little-endian, i.e. LSB.
        /// </summary>
        LE = 1
    }
}