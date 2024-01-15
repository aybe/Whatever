using System.Security.Cryptography;

namespace Whatever.Interop.Tests.NewFolder;

public static class StreamExtensions
{
    public static string GetSha256Hash(this Stream stream, long? position = 0)
    {
        using var scope = new StreamPositionScope(stream, position);

        var data = SHA256.HashData(stream);

        var hash = string.Concat(data.Select(s => s.ToString("x2")));

        return hash;
    }
}