using Whatever.Extensions;
using Whatever.ISO9660.Logical;

namespace Whatever.ISO9660.Physical;

public static class DiscExtensions
{
    public static async Task ReadFileRawAsync(this Disc disc, IsoFileSystemEntryFile file, Stream stream, IProgress<double>? progress = null)
    {
        await ReadFileAsync(disc, file, stream, ReadFileRaw, progress).ConfigureAwait(false);
    }

    public static async Task ReadFileUserAsync(this Disc disc, IsoFileSystemEntryFile file, Stream stream, IProgress<double>? progress = null)
    {
        await ReadFileAsync(disc, file, stream, ReadFileUser, progress).ConfigureAwait(false);
    }

    private static async Task ReadFileAsync(Disc disc, IsoFileSystemEntryFile file, Stream stream, ReadFileHandler handler, IProgress<double>? progress)
    {
        var position = (int)file.Position;

        var track = disc.Tracks.FirstOrDefault(s => position >= s.Position)
                    ?? throw new InvalidOperationException("Failed to determine track for file.");

        var sectors = (int)Math.Ceiling((double)file.Length / track.Sector.GetUserDataLength());

        using var buffer = new ArrayPoolScope<byte>(2352);

        for (var i = 0; i < sectors; i++)
        {
            var sector = await track.ReadSectorAsync(i + position).ConfigureAwait(false);

            var length = handler(file, stream, sector, buffer);

            await stream.WriteAsync(buffer.Memory[..length]).ConfigureAwait(false);

            progress?.Report(1.0d / sectors * (i + 1));
        }
    }

    private static int ReadFileRaw(IsoFileSystemEntryFile file, Stream stream, ISector sector, ArrayPoolScope<byte> buffer)
    {
        var data = sector.GetData();

        var size = data.Length;

        var span = data[..size];

        span.CopyTo(buffer);

        return size;
    }

    private static int ReadFileUser(IsoFileSystemEntryFile file, Stream stream, ISector sector, ArrayPoolScope<byte> buffer)
    {
        var data = sector.GetUserData();

        var size = (int)Math.Min(Math.Max(file.Length - stream.Length, 0), data.Length);

        var span = data[..size];

        span.CopyTo(buffer);

        return size;
    }

    private delegate int ReadFileHandler(IsoFileSystemEntryFile file, Stream stream, ISector sector, ArrayPoolScope<byte> buffer);
}