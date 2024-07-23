using Whatever.Extensions;

namespace Whatever.ISO9660.Logical;

public sealed class IsoVolumeDescriptorSetTerminator(IsoVolumeDescriptor descriptor, Stream stream)
    : IsoVolumeDescriptor(descriptor)
{
    public byte[] Reserved { get; } = stream.ReadExactly(2041);
}