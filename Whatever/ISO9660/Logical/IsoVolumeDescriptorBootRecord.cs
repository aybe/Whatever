using Whatever.Extensions;

namespace Whatever.ISO9660.Logical;

public sealed class IsoVolumeDescriptorBootRecord(IsoVolumeDescriptor descriptor, Stream stream)
    : IsoVolumeDescriptor(descriptor)
{
    public string BootSystemIdentifier { get; } = stream.ReadIsoString(32, IsoStringFlags.ACharacters);

    public string BootIdentifier { get; } = stream.ReadIsoString(32, IsoStringFlags.ACharacters);

    public byte[] BootSystemUse { get; } = stream.ReadExactly(1977);
}