using System.Collections.ObjectModel;
using System.Linq;

namespace ISO9660.Logical;

public sealed class VolumeDescriptorSet : Collection<VolumeDescriptor>
{
    public VolumeDescriptorPrimary PrimaryVolumeDescriptor => this.OfType<VolumeDescriptorPrimary>().Single();
}