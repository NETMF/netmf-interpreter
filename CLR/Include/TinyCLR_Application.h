////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#ifndef _TINYCLR_APPLICATION_H_
#define _TINYCLR_APPLICATION_H_

#if defined(_WIN32) || defined(WIN32)
#define PLATFORM_WINDOWS
#endif

extern void InitCRuntime();

struct CLR_SETTINGS
{
    unsigned short  MaxContextSwitches;
    bool            WaitForDebugger;
    bool            EnterDebuggerLoopAfterExit;

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
    bool            PerformGarbageCollection;
    bool            PerformHeapCompaction;
    WCHAR           EmulatorArgs[1024];
    void*           WinSettings;
#endif
};

struct CLR_RT_EmulatorHooks
{
#if defined(PLATFORM_WINDOWS)
    static void Notify_ExecutionStateChanged();
#else
    static void Notify_ExecutionStateChanged() {}
#endif
};

extern void ClrStartup( CLR_SETTINGS params );

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)
extern HRESULT ClrLoadPE( LPCWSTR szPeFilePath );
extern HRESULT ClrLoadDAT( LPCWSTR szDatFilePath );
void ClrSetLcdDimensions( INT32 width, INT32 height, INT32 bitsPerPixel );
bool ClrIsDebuggerStopped();
#endif

extern void ClrExit();

#if defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)

struct HAL_Configuration_Windows
{    
    enum Product
    {
        Product_Aux  ,
        Product_Digi,
    };

    enum Buttons
    {
        Buttons_Legacy = 1,
        Buttons_Native = 2,
    };

    int SystemClock;
    int SlowClockPerSecond;   
    int LCD_Width;
    int LCD_Height;
    int LCD_BitsPerPixel;    
    int LCD_Orientation;
    INT64 TicksPerOpcode;
    INT64 TicksPerMethodCall;
    Product ProductType;
    double RDTSC_To_PerfCount_Ratio;
    bool GraphHeapEnabled;


    HRESULT InitializeForProduct( Product product );
};

extern HAL_Configuration_Windows g_HAL_Configuration_Windows;

#endif //defined(PLATFORM_WINDOWS) || defined(PLATFORM_WINCE)

#endif //_TINYCLR_APPLICATION_H_
