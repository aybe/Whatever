using System.Runtime.Versioning;
using Microsoft.Win32.SafeHandles;
using Whatever.ISO9660.Extensions;

namespace Whatever.ISO9660.Physical;

internal sealed class TrackRaw(int index, int position, int length, bool audio, ISector sector, SafeFileHandle handle)
    : Track(audio, index, length, position, sector)
{
    private NativeMemory<byte> Buffer { get; } = Disc.GetDeviceAlignedBuffer(2352, handle);

    private SafeFileHandle Handle { get; } = handle;

    public override ISector ReadSector(int index)
    {
        if (OperatingSystem.IsWindows())
        {
            return ReadSectorWindows(index);
        }

        throw new PlatformNotSupportedException();
    }

    protected override void DisposeManaged()
    {
        Buffer.Dispose();
    }

    public override Task<ISector> ReadSectorAsync(int index)
    {
        if (OperatingSystem.IsWindows())
        {
            return ReadSectorWindowsAsync(index);
        }

        throw new PlatformNotSupportedException();
    }

    [SupportedOSPlatform("windows")]
    private ISector ReadSectorWindows(int index)
    {
        var buffer = Buffer.Manager.Memory.Span;

        Disc.ReadSector(Handle, (uint)index, buffer);

        var sector = ISector.Read(Sector, buffer);

        return sector;
    }

    [SupportedOSPlatform("windows")]
    private async Task<ISector> ReadSectorWindowsAsync(int index, uint timeout = 3u) // TODO move to Disc?
    {
        using var query = Disc.ReadSectorWindowsQuery((uint)index, 1u, timeout, Buffer.Pointer, Buffer.Length);

        await DeviceIoControl.SendAsync(
            Handle, NativeConstants.IOCTL_SCSI_PASS_THROUGH_DIRECT, query, query, TimeSpan.FromSeconds(timeout));

        var sector = ISector.Read(Sector, Buffer.Manager.Memory.Span);

        return sector;
    }
}