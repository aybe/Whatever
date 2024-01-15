using System.Diagnostics.CodeAnalysis;

namespace Whatever.Interop.Tests.NewFolder;

[SuppressMessage("ReSharper", "UnassignedReadonlyField")]
public readonly struct SectorSubHeader : IEquatable<SectorSubHeader>
{
    public readonly byte FileNumber;
    public readonly byte ChannelNumber;
    public readonly SectorSubMode SubMode;
    public readonly byte CodingInformation;

    public bool IsRealTimeAudio
    {
        get
        {
            var b = SubMode.HasFlags(SectorSubMode.Audio | SectorSubMode.Form | SectorSubMode.RealTimeSector);

            return b;
        }
    }

    public bool IsRealTimeVideo // Data -> STRv3
    {
        get
        {
            var b = SubMode.HasFlags(SectorSubMode.RealTimeSector) &&
                    (SubMode.HasFlags(SectorSubMode.Data) || SubMode.HasFlags(SectorSubMode.Video));

            return b;
        }
    }

    public bool TryGetAudioBitsPerSample(out int result)
    {
        result = default;

        if (IsRealTimeAudio is false)
        {
            return false;
        }

        switch ((CodingInformation & 0b00110000) >> 4)
        {
            case 0:
                result = 4;
                return true;
            default:
                return false;
        }
    }

    public bool TryGetAudioChannels(out int result)
    {
        result = default;

        if (IsRealTimeAudio is false)
        {
            return false;
        }

        switch ((CodingInformation & 0b00000011) >> 0)
        {
            case 0:
                result = 1;
                return true;
            case 1:
                result = 2;
                return true;
            default:
                return false;
        }
    }

    public bool TryGetAudioSampleRate(out int result)
    {
        result = default;

        if (IsRealTimeAudio is false)
        {
            return false;
        }

        switch ((CodingInformation & 0b00001100) >> 2)
        {
            case 0:
                result = 37800;
                return true;
            case 1:
                result = 18900;
                return true;
            default:
                return false;
        }
    }

    public override string ToString()
    {
        return
            $"{nameof(FileNumber)}: {FileNumber}, {nameof(ChannelNumber)}: {ChannelNumber}, {nameof(SubMode)}: {SubMode}, {nameof(CodingInformation)}: {CodingInformation}";
    }

    #region Equality members

    public bool Equals(SectorSubHeader other)
    {
        return
            FileNumber == other.FileNumber &&
            ChannelNumber == other.ChannelNumber &&
            SubMode == other.SubMode &&
            CodingInformation == other.CodingInformation;
    }

    public override bool Equals(object? obj)
    {
        return obj is SectorSubHeader other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FileNumber, ChannelNumber, SubMode, CodingInformation);
    }

    public static bool operator ==(SectorSubHeader left, SectorSubHeader right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(SectorSubHeader left, SectorSubHeader right)
    {
        return !left.Equals(right);
    }

    #endregion
}