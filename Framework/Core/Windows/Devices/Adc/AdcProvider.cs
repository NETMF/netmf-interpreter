using System;

namespace Windows.Devices.Adc.Provider
{
    public interface IAdcControllerProvider
    {
        int ChannelCount
        {
            get;
        }

        int ResolutionInBits
        {
            get;
        }

        int MinValue
        {
            get;
        }

        int MaxValue
        {
            get;
        }

        ProviderAdcChannelMode ChannelMode
        {
            get;
            set;
        }

        bool IsChannelModeSupported(ProviderAdcChannelMode channelMode);
        void AcquireChannel(int channel);
        void ReleaseChannel(int channel);
        int ReadValue(int channelNumber);
    }

    public interface IAdcProvider
    {
        IAdcControllerProvider[] GetControllers();
    }
}
