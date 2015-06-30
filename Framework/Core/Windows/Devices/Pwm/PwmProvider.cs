using System;

namespace Windows.Devices.Pwm.Provider
{
    public interface IPwmControllerProvider
    {
        int PinCount
        {
            get;
        }

        double MinFrequency
        {
            get;
        }

        double MaxFrequency
        {
            get;
        }

        double ActualFrequency
        {
            get;
        }

        double SetDesiredFrequency(double frequency);
        void AcquirePin(int pin);
        void ReleasePin(int pin);
        void EnablePin(int pin);
        void DisablePin(int pin);
        void SetPulseParameters(int pin, double dutyCycle, bool invertPolarity);
    }

    public interface IPwmProvider
    {
        // FUTURE: This should return "IReadOnlyList<IPwmControllerProvider>"
        IPwmControllerProvider[] GetControllers();
    }
}
