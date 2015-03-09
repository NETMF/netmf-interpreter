////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_HARDWARE_NATIVE_H_
#define _SPOT_HARDWARE_NATIVE_H_

#include <TinyCLR_Interop.h>

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_Cpu
{
    TINYCLR_NATIVE_DECLARE(get_SystemClock___STATIC__U4);
    TINYCLR_NATIVE_DECLARE(get_SlowClock___STATIC__U4);
    TINYCLR_NATIVE_DECLARE(get_GlitchFilterTime___STATIC__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(set_GlitchFilterTime___STATIC__VOID__mscorlibSystemTimeSpan);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery
{
    TINYCLR_NATIVE_DECLARE(ReadVoltage___STATIC__I4);
    TINYCLR_NATIVE_DECLARE(ReadTemperature___STATIC__I4);
    TINYCLR_NATIVE_DECLARE(OnCharger___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(IsFullyCharged___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(StateOfCharge___STATIC__I4);
    TINYCLR_NATIVE_DECLARE(WaitForEvent___STATIC__BOOLEAN__I4);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_Battery__ChargerModel
{
    static const int FIELD__Charge_Min        = 1;
    static const int FIELD__Charge_Low        = 2;
    static const int FIELD__Charge_Medium     = 3;
    static const int FIELD__Charge_Full       = 4;
    static const int FIELD__Charge_FullMin    = 5;
    static const int FIELD__Charge_Hysteresis = 6;
    static const int FIELD__Timeout_Charging  = 7;
    static const int FIELD__Timeout_Charged   = 8;
    static const int FIELD__Timeout_Charger   = 9;
    static const int FIELD__Timeout_Backlight = 10;

    TINYCLR_NATIVE_DECLARE(_ctor___VOID);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice
{
    static const int FIELD__m_xAction  = 1;
    static const int FIELD__Config     = 2;
    static const int FIELD__m_disposed = 3;

    TINYCLR_NATIVE_DECLARE(Execute___I4__SZARRAY_MicrosoftSPOTHardwareI2CDeviceI2CTransaction__I4);
    TINYCLR_NATIVE_DECLARE(Initialize___VOID);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__Configuration
{
    static const int FIELD__Address      = 1;
    static const int FIELD__ClockRateKhz = 2;

    //--//

    static const int c_MaxI2cAddress       = 0x7F;
    static const int c_MaximumClockRateKhz =  400;
    static const int c_MimimumClockRateKhz =   10;

    //--//
        
    static HRESULT GetInitialConfig(CLR_RT_HeapBlock& ref, I2C_USER_CONFIGURATION& nativeConfig);
};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI
{
    static const int FIELD__m_config = 1;
    static const int FIELD__m_cs = 2;
    static const int FIELD__m_disposed = 3;

    TINYCLR_NATIVE_DECLARE(InternalWriteRead___VOID__SZARRAY_U2__I4__I4__SZARRAY_U2__I4__I4__I4);
    TINYCLR_NATIVE_DECLARE(InternalWriteRead___VOID__SZARRAY_U1__I4__I4__SZARRAY_U1__I4__I4__I4);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_SPI__Configuration
{
    static const int FIELD__ChipSelect_Port = 1;
    static const int FIELD__ChipSelect_ActiveState = 2;
    static const int FIELD__ChipSelect_SetupTime = 3;
    static const int FIELD__ChipSelect_HoldTime = 4;
    static const int FIELD__Clock_IdleState = 5;
    static const int FIELD__Clock_Edge = 6;
    static const int FIELD__Clock_RateKHz = 7;
    static const int FIELD__SPI_mod = 8;
    static const int FIELD__BusyPin = 9;
    static const int FIELD__BusyPin_ActiveState = 10;

    //--//
        
    static HRESULT GetInitialConfig(CLR_RT_HeapBlock& ref, SPI_CONFIGURATION& nativeConfig);
};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_NativeEventDispatcher
{
    static const int FIELD__m_threadSpawn           = 1;
    static const int FIELD__m_callbacks             = 2;
    static const int FIELD__m_disposed              = 3;
    static const int FIELD__m_NativeEventDispatcher = 4;

    TINYCLR_NATIVE_DECLARE(EnableInterrupt___VOID);
    TINYCLR_NATIVE_DECLARE(DisableInterrupt___VOID);
    TINYCLR_NATIVE_DECLARE(Dispose___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__STRING__U8);

    //--//
    static CLR_RT_ObjectToEvent_Source*             GetEventDispReference( CLR_RT_StackFrame& stack                                  );
    static HRESULT                                  GetEventDispatcher   ( CLR_RT_StackFrame& stack, CLR_RT_HeapBlock_NativeEventDispatcher*& port  );
    static CLR_RT_HeapBlock_NativeEventDispatcher*  GetEventDispatcher   ( CLR_RT_StackFrame& stack                                  );

};

struct Library_spot_hardware_native_Microsoft_SPOT_EventSink
{
    static const int FIELD_STATIC___eventSink = 0;
    static const int FIELD_STATIC___eventInfoTable = 1;

    TINYCLR_NATIVE_DECLARE(EventConfig___VOID);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_EventSink__EventInfo
{
    static const int FIELD__EventListener = 1;
    static const int FIELD__EventFilter = 2;
    static const int FIELD__EventProcessor = 3;
    static const int FIELD__Category = 4;


    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_I2CDevice__I2CTransaction
{
    static const int FIELD__Buffer = 1;


    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_Port
{
    static const int FIELD__m_interruptMode      = 5;
    static const int FIELD__m_resistorMode       = 6;
    static const int FIELD__m_portId             = 7;
    static const int FIELD__m_flags              = 8;
    static const int FIELD__m_glitchFilterEnable = 9;
    static const int FIELD__m_initialState       = 10;

    TINYCLR_NATIVE_DECLARE(Dispose___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN__MicrosoftSPOTHardwarePortResistorMode__MicrosoftSPOTHardwarePortInterruptMode);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(_ctor___VOID__MicrosoftSPOTHardwareCpuPin__BOOLEAN__BOOLEAN__MicrosoftSPOTHardwarePortResistorMode);
    TINYCLR_NATIVE_DECLARE(Read___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_Id___MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(ReservePin___STATIC__BOOLEAN__MicrosoftSPOTHardwareCpuPin__BOOLEAN);

    //--//

    static HRESULT ChangeState( CLR_RT_HeapBlock* pThis, CLR_RT_HeapBlock_NativeEventDispatcher* pIOPort, bool toOutput );
    static void IsrProcedure( GPIO_PIN pin, BOOL pinState, void* context );
};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBufferMarshaller
{
    static const int FIELD_STATIC__m_initialized = 2;
    static const int FIELD_STATIC__OnLargeBufferRequest = 3;

    static const int FIELD__m_id = 1;

    TINYCLR_NATIVE_DECLARE(MarshalBuffer___VOID__MicrosoftSPOTHardwareLargeBuffer);
    TINYCLR_NATIVE_DECLARE(UnMarshalBuffer___VOID__BYREF_MicrosoftSPOTHardwareLargeBuffer);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogInput
{
    static const int FIELD_STATIC__s_syncRoot = 4;

    static const int FIELD__m_pin = 1;
    static const int FIELD__m_channel = 2;
    static const int FIELD__m_scale = 3;
    static const int FIELD__m_offset = 4;
    static const int FIELD__m_precision = 5;
    static const int FIELD__m_disposed = 6;

    TINYCLR_NATIVE_DECLARE(ReadRaw___I4);
    TINYCLR_NATIVE_DECLARE(Initialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogChannel__I4);
    TINYCLR_NATIVE_DECLARE(Uninitialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogChannel);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_AnalogOutput
{
    static const int FIELD_STATIC__s_syncRoot = 5;

    static const int FIELD__m_pin = 1;
    static const int FIELD__m_channel = 2;
    static const int FIELD__m_scale = 3;
    static const int FIELD__m_offset = 4;
    static const int FIELD__m_precision = 5;

    TINYCLR_NATIVE_DECLARE(WriteRaw___VOID__I4);
    TINYCLR_NATIVE_DECLARE(Initialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogOutputChannel__I4);
    TINYCLR_NATIVE_DECLARE(Uninitialize___STATIC__VOID__MicrosoftSPOTHardwareCpuAnalogOutputChannel);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_HardwareProvider
{
    static const int FIELD_STATIC__s_hwProvider = 6;

    TINYCLR_NATIVE_DECLARE(NativeGetSerialPins___VOID__I4__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetSerialPortsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeSupportsNonStandardBaudRate___BOOLEAN__I4);
    TINYCLR_NATIVE_DECLARE(NativeGetBaudRateBoundary___VOID__I4__BYREF_U4__BYREF_U4);
    TINYCLR_NATIVE_DECLARE(NativeIsSupportedBaudRate___BOOLEAN__I4__BYREF_U4);
    TINYCLR_NATIVE_DECLARE(NativeGetSpiPins___VOID__MicrosoftSPOTHardwareSPISPImodule__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetSpiPortsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeGetI2CPins___VOID__BYREF_MicrosoftSPOTHardwareCpuPin__BYREF_MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetPinsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeGetPinsMap___VOID__SZARRAY_MicrosoftSPOTHardwareCpuPinUsage);
    TINYCLR_NATIVE_DECLARE(NativeGetPinUsage___MicrosoftSPOTHardwareCpuPinUsage__MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetSupportedResistorModes___MicrosoftSPOTHardwareCpuPinValidResistorMode__MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetSupportedInterruptModes___MicrosoftSPOTHardwareCpuPinValidInterruptMode__MicrosoftSPOTHardwareCpuPin);
    TINYCLR_NATIVE_DECLARE(NativeGetButtonPins___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareButton);
    TINYCLR_NATIVE_DECLARE(NativeGetLCDMetrics___VOID__BYREF_I4__BYREF_I4__BYREF_I4__BYREF_I4);
    TINYCLR_NATIVE_DECLARE(NativeGetPWMChannelsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeGetPWMPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuPWMChannel);
    TINYCLR_NATIVE_DECLARE(NativeGetAnalogChannelsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeGetAnalogPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuAnalogChannel);
    TINYCLR_NATIVE_DECLARE(NativeGetAvailablePrecisionInBitsForChannel___SZARRAY_I4__MicrosoftSPOTHardwareCpuAnalogChannel);
    TINYCLR_NATIVE_DECLARE(NativeGetAnalogOutputChannelsCount___I4);
    TINYCLR_NATIVE_DECLARE(NativeGetAnalogOutputPinForChannel___MicrosoftSPOTHardwareCpuPin__MicrosoftSPOTHardwareCpuAnalogOutputChannel);    
    TINYCLR_NATIVE_DECLARE(NativeGetAvailableAnalogOutputPrecisionInBitsForChannel___SZARRAY_I4__MicrosoftSPOTHardwareCpuAnalogOutputChannel);

    //--//


};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_InputPort
{
    TINYCLR_NATIVE_DECLARE(get_Resistor___MicrosoftSPOTHardwarePortResistorMode);
    TINYCLR_NATIVE_DECLARE(set_Resistor___VOID__MicrosoftSPOTHardwarePortResistorMode);
    TINYCLR_NATIVE_DECLARE(get_GlitchFilter___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(set_GlitchFilter___VOID__BOOLEAN);

    //--//

};


struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_InterruptPort
{
    TINYCLR_NATIVE_DECLARE(EnableInterrupt___VOID);
    TINYCLR_NATIVE_DECLARE(DisableInterrupt___VOID);
    TINYCLR_NATIVE_DECLARE(ClearInterrupt___VOID);
    TINYCLR_NATIVE_DECLARE(get_Interrupt___MicrosoftSPOTHardwarePortInterruptMode);
    TINYCLR_NATIVE_DECLARE(set_Interrupt___VOID__MicrosoftSPOTHardwarePortInterruptMode);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_LargeBuffer
{
    static const int FIELD__m_bytes = 1;
    static const int FIELD__m_fDisposed = 2;

    TINYCLR_NATIVE_DECLARE(InternalCreateBuffer___VOID__I4);
    TINYCLR_NATIVE_DECLARE(InternalDestroyBuffer___VOID);

    //--//

    static HRESULT CreateBufferHelper(CLR_RT_HeapBlock& hbBytes, size_t size);

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_OutputPort
{
    TINYCLR_NATIVE_DECLARE(Write___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_InitialState___BOOLEAN);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerEvent
{
    static const int FIELD__EventType = 3;
    static const int FIELD__Level     = 4;
    static const int FIELD__Time      = 5;


    //--//

};


struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_PowerState
{
    static const int FIELD_STATIC__s_CurrentPowerLevel = 7;
    static const int FIELD_STATIC__OnSleepChange = 8;
    static const int FIELD_STATIC__OnPowerLevelChange = 9;
    static const int FIELD_STATIC__OnRebootEvent = 10;

    TINYCLR_NATIVE_DECLARE(Reboot___STATIC__VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(WaitForIdleCPU___STATIC__BOOLEAN__I4__I4);
    TINYCLR_NATIVE_DECLARE(get_MaximumTimeToActive___STATIC__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(set_MaximumTimeToActive___STATIC__VOID__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(get_WakeupEvents___STATIC__MicrosoftSPOTHardwareHardwareEvent);
    TINYCLR_NATIVE_DECLARE(set_WakeupEvents___STATIC__VOID__MicrosoftSPOTHardwareHardwareEvent);
    TINYCLR_NATIVE_DECLARE(get_Uptime___STATIC__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(InternalSleep___STATIC__VOID__MicrosoftSPOTHardwareSleepLevel__MicrosoftSPOTHardwareHardwareEvent);
    TINYCLR_NATIVE_DECLARE(InternalChangePowerLevel___STATIC__VOID__MicrosoftSPOTHardwarePowerLevel);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_SleepEvent
{
    static const int FIELD__EventType    = 3;
    static const int FIELD__Level        = 4;
    static const int FIELD__WakeUpEvents = 5;
    static const int FIELD__Time         = 6;


    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_TristatePort
{
    TINYCLR_NATIVE_DECLARE(get_Active___BOOLEAN);
    TINYCLR_NATIVE_DECLARE(set_Active___VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_Resistor___MicrosoftSPOTHardwarePortResistorMode);
    TINYCLR_NATIVE_DECLARE(set_Resistor___VOID__MicrosoftSPOTHardwarePortResistorMode);
    TINYCLR_NATIVE_DECLARE(get_GlitchFilter___BOOLEAN);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_Watchdog
{
    TINYCLR_NATIVE_DECLARE(get_Enabled___STATIC__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(set_Enabled___STATIC__VOID__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(get_Timeout___STATIC__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(set_Timeout___STATIC__VOID__mscorlibSystemTimeSpan);
    TINYCLR_NATIVE_DECLARE(get_Behavior___STATIC__MicrosoftSPOTHardwareWatchdogBehavior);
    TINYCLR_NATIVE_DECLARE(set_Behavior___STATIC__VOID__MicrosoftSPOTHardwareWatchdogBehavior);
    TINYCLR_NATIVE_DECLARE(get_Log___STATIC__MicrosoftSPOTNativeMicrosoftSPOTILog);
    TINYCLR_NATIVE_DECLARE(set_Log___STATIC__VOID__MicrosoftSPOTNativeMicrosoftSPOTILog);
    TINYCLR_NATIVE_DECLARE(GetLastOcurrenceDetails___STATIC__BOOLEAN__BYREF_mscorlibSystemDateTime__BYREF_mscorlibSystemTimeSpan__BYREF_mscorlibSystemReflectionMethodInfo);

    //--//

};

struct Library_spot_hardware_native_Microsoft_SPOT_Hardware_WatchdogEvent
{
    static const int FIELD__WatchdogEventTime    = 1;
    static const int FIELD__WatchdogTimeoutValue = 2;
    static const int FIELD__OffendingMethod      = 3;


    //--//

};

struct Library_spot_hardware_native_System_IO_Ports_SerialDataReceivedEventArgs
{
    static const int FIELD__m_data = 1;


    //--//

};

struct Library_spot_hardware_native_System_IO_Ports_SerialErrorReceivedEventArgs
{
    static const int FIELD__m_error = 1;


    //--//

};

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_EventSink_DriverProcs;

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware;

#endif  //_SPOT_HARDWARE_NATIVE_H_

