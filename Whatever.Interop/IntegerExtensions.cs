using System;

namespace Whatever.Interop
{
    public static class IntegerExtensions // TODO DRY
    {
        public static uint ToUInt32(this int value)
        {
            return Convert.ToUInt32(value);
        }
    }
}