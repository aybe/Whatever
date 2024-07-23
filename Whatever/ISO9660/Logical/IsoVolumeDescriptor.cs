using Whatever.Extensions;

namespace Whatever.ISO9660.Logical;

public class IsoVolumeDescriptor
{
    internal IsoVolumeDescriptor(Stream stream)
    {
        VolumeDescriptorType    = stream.Read<IsoVolumeDescriptorType>(); // 711
        StandardIdentifier      = stream.ReadStringAscii(5);
        VolumeDescriptorVersion = stream.ReadIso711();
    }

    protected IsoVolumeDescriptor(IsoVolumeDescriptor descriptor)
    {
        VolumeDescriptorType    = descriptor.VolumeDescriptorType;
        StandardIdentifier      = descriptor.StandardIdentifier;
        VolumeDescriptorVersion = descriptor.VolumeDescriptorVersion;
    }

    public IsoVolumeDescriptorType VolumeDescriptorType { get; }

    public string StandardIdentifier { get; }

    public byte VolumeDescriptorVersion { get; }
}