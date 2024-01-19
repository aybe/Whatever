using System;
using System.Globalization;

namespace Whatever.Extensions
{
    /// <summary>
    ///     Extension methods for <see cref="Enum" />.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        ///     Gets whether an enum has specified flags.
        /// </summary>
        public static bool HasFlags<T>(this T value, T flags) where T : Enum
        {
            var a = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            var b = Convert.ToUInt64(flags, CultureInfo.InvariantCulture);
            var c = (a & b) == b;

            return c;
        }
    }
}