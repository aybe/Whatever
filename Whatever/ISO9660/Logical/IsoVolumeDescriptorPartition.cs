using Whatever.Extensions;

namespace Whatever.ISO9660.Logical;

public sealed class IsoVolumeDescriptorPartition : IsoVolumeDescriptor
{
    public IsoVolumeDescriptorPartition(IsoVolumeDescriptor descriptor, Stream stream)
        : base(descriptor)
    {
        stream.Seek(1, SeekOrigin.Current);

        SystemIdentifier = stream.ReadIsoString(32, IsoStringFlags.ACharacters);

        VolumePartitionIdentifier = stream.ReadIsoString(32, IsoStringFlags.DCharacters);

        VolumePartitionLocation = stream.ReadIso733();

        VolumePartitionSize = stream.ReadIso733();

        SystemUse = stream.ReadExactly(1960);
    }

    public string SystemIdentifier { get; }

    public string VolumePartitionIdentifier { get; }

    public uint VolumePartitionLocation { get; }

    public uint VolumePartitionSize { get; }

    public byte[] SystemUse { get; }
}