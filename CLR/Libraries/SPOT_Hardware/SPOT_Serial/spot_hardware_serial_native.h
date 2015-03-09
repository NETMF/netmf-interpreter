////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#ifndef _SPOT_HARDWARE_SERIAL_NATIVE_H_
#define _SPOT_HARDWARE_SERIAL_NATIVE_H_

//--//

#include <TinyCLR_Interop.h>

//--//

struct Library_spot_hardware_serial_native_System_IO_Ports_SerialErrorReceivedEventArgs
{
    static const int FIELD__m_error = 1;


    //--//

};

struct Library_spot_hardware_serial_native_System_IO_Ports_SerialDataReceivedEventArgs
{
    static const int FIELD__m_data = 1;


    //--//

};

struct Library_spot_hardware_serial_native_System_IO_Ports_SerialPort
{
    static const int FIELD__m_config              = 1;
    static const int FIELD__m_fDisposed           = 2;
    static const int FIELD__m_fOpened             = 3;
    static const int FIELD__m_portName            = 4;
    static const int FIELD__m_evtErrorEvent       = 5;
    static const int FIELD__m_evtDataEvent        = 6;
    static const int FIELD__m_callbacksErrorEvent = 7;
    static const int FIELD__m_callbacksDataEvent  = 8;

    TINYCLR_NATIVE_DECLARE(Read___I4__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(Write___VOID__SZARRAY_U1__I4__I4);
    TINYCLR_NATIVE_DECLARE(Flush___VOID);
    TINYCLR_NATIVE_DECLARE(InternalOpen___VOID);
    TINYCLR_NATIVE_DECLARE(InternalClose___VOID);
    TINYCLR_NATIVE_DECLARE(InternalDispose___VOID);
    TINYCLR_NATIVE_DECLARE(BytesInBuffer___I4__BOOLEAN);
    TINYCLR_NATIVE_DECLARE(DiscardBuffer___VOID__BOOLEAN);

    //--//

};

struct Library_spot_hardware_serial_native_System_IO_Ports_SerialPort__Configuration
{
    static const int FIELD__PortIndex    = 1;
    static const int FIELD__Speed        = 2;
    static const int FIELD__Parity       = 3;
    static const int FIELD__DataBits     = 4;
    static const int FIELD__StopBits     = 5;
    static const int FIELD__Handshake    = 6;
    static const int FIELD__ReadTimeout  = 7;
    static const int FIELD__WriteTimeout = 8;


    //--//

};

//--//

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartError;
extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_UsartEvent;

//--//

extern const CLR_RT_NativeAssemblyData g_CLR_AssemblyNative_Microsoft_SPOT_Hardware_SerialPort;

//--//

#endif  //_SPOT_HARDWARE_SERIAL_NATIVE_H_


