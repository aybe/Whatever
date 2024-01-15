namespace Whatever.Interop.Tests.NewFolder;

public static class EnumExtensions
{
    public static bool HasFlags<T>(this T value, in T flags) where T : Enum
    {
        var a = Convert.ToUInt64(value);
        var b = Convert.ToUInt64(flags);
        var c = (a & b) == b;

        return c;
    }
}