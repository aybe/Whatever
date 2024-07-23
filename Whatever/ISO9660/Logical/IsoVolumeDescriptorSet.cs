using System.Collections.ObjectModel;

namespace Whatever.ISO9660.Logical;

public sealed class IsoVolumeDescriptorSet : Collection<IsoVolumeDescriptor>
{
    public IsoVolumeDescriptorPrimary PrimaryVolumeDescriptor => this.OfType<IsoVolumeDescriptorPrimary>().Single();
}