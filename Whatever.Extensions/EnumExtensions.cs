using System;
using System.Globalization;

namespace Whatever.Extensions
{
    public static class EnumExtensions
    {
        public static bool HasFlags<T>(this T value, T flags) where T : Enum
        {
            var a = Convert.ToUInt64(value, CultureInfo.InvariantCulture);
            var b = Convert.ToUInt64(flags, CultureInfo.InvariantCulture);
            var c = (a & b) == b;

            return c;
        }
    }
}