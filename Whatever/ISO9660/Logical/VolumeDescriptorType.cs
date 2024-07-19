namespace Whatever.ISO9660.Logical;

#pragma warning disable CA1028 // Enum Storage should be Int32
public enum VolumeDescriptorType : byte
#pragma warning restore CA1028 // Enum Storage should be Int32
{
    BootRecord = 0,
    PrimaryVolumeDescriptor = 1,
    SupplementaryVolumeDescriptor = 2,
    VolumePartitionDescriptor = 3,
    VolumeDescriptorSetTerminator = 255
}