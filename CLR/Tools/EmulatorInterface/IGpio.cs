using System;

namespace Microsoft.SPOT.Emulator.Gpio
{
    public interface IGpioDriver
    {
        bool Initialize();
        bool Uninitialize();
        uint Attributes( uint pin );
        void DisablePin( uint pin, int resistorState, uint direction, uint altFunction );
        void EnableOutputPin( uint pin, bool initialState );
        bool EnableInputPin( uint pin, bool glitchFilterEnable, IntPtr isr, int interruptEdge, int resistorState );
        bool EnableInputPin( uint pin, bool glitchFilterEnable, IntPtr isr, IntPtr isrParam, int interruptEdge, int resistorState );
        bool GetPinState( uint pin );
        void SetPinState( uint pin, bool pinState );
        bool PinIsBusy( uint pin );
        bool ReservePin( uint pin, bool reserve );
        uint GetDebounce();
        bool SetDebounce( long debounceTime ); 
        int  GetPinCount();
        void GetPinsMap( IntPtr pins, int size );
        byte GetSupportedResistorModes( uint pin );
        byte GetSupportedInterruptModes( uint pin );
        int  GetVirtualKeyPin(uint virtualKey);
    }
}
