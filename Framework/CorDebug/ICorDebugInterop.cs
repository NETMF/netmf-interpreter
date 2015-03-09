using CorDebugInterop;
using System;
using System.Runtime.InteropServices;
using Microsoft.SPOT.Debugger;

namespace CorDebugInterop
{
#pragma warning disable 0108

    [Guid("3D6F5F61-7538-11D3-8D5B-00104B35E7EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebug
    {
        [PreserveSig]int Initialize();
        [PreserveSig]int Terminate();
        [PreserveSig]int SetManagedHandler([In, MarshalAs(UnmanagedType.Interface)]ICorDebugManagedCallback pCallback);
        [PreserveSig]int SetUnmanagedHandler([In, MarshalAs(UnmanagedType.Interface)]ICorDebugUnmanagedCallback pCallback);
        [PreserveSig]int CreateProcess([In, MarshalAs(UnmanagedType.LPWStr)]string lpApplicationName, [In, MarshalAs(UnmanagedType.LPWStr)]string lpCommandLine, [In]_SECURITY_ATTRIBUTES lpProcessAttributes, [In]_SECURITY_ATTRIBUTES lpThreadAttributes, [In]int bInheritHandles, [In]uint dwCreationFlags, [In]IntPtr lpEnvironment, [In, MarshalAs(UnmanagedType.LPWStr)]string lpCurrentDirectory, [In]_STARTUPINFO lpStartupInfo, [In]_PROCESS_INFORMATION lpProcessInformation, [In]CorDebugCreateProcessFlags debuggingFlags, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int DebugActiveProcess([In]uint id, [In]int win32Attach, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int EnumerateProcesses([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcessEnum ppProcess);
        [PreserveSig]int GetProcess([In]uint dwProcessId, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int CanLaunchOrAttach([In]uint dwProcessId, [In]int win32DebuggingEnabled);
    }

    public enum CorDebugInterfaceVersion : int
    {
        CorDebugInvalidVersion            = 0,
        CorDebugVersion_1_0               = CorDebugInvalidVersion + 1,
        
        ver_ICorDebugManagedCallback      = CorDebugVersion_1_0,
        ver_ICorDebugUnmanagedCallback    = CorDebugVersion_1_0,
        ver_ICorDebug                     = CorDebugVersion_1_0,
        ver_ICorDebugController           = CorDebugVersion_1_0,
        ver_ICorDebugAppDomain            = CorDebugVersion_1_0,
        ver_ICorDebugAssembly             = CorDebugVersion_1_0,
        ver_ICorDebugProcess              = CorDebugVersion_1_0,
        ver_ICorDebugBreakpoint           = CorDebugVersion_1_0,
        ver_ICorDebugFunctionBreakpoint   = CorDebugVersion_1_0,
        ver_ICorDebugModuleBreakpoint     = CorDebugVersion_1_0,
        ver_ICorDebugValueBreakpoint      = CorDebugVersion_1_0,
        ver_ICorDebugStepper              = CorDebugVersion_1_0,
        ver_ICorDebugRegisterSet          = CorDebugVersion_1_0,
        ver_ICorDebugThread               = CorDebugVersion_1_0,
        ver_ICorDebugChain                = CorDebugVersion_1_0,
        ver_ICorDebugFrame                = CorDebugVersion_1_0,
        ver_ICorDebugILFrame              = CorDebugVersion_1_0,
        ver_ICorDebugNativeFrame          = CorDebugVersion_1_0,
        ver_ICorDebugModule               = CorDebugVersion_1_0,
        ver_ICorDebugFunction             = CorDebugVersion_1_0,
        ver_ICorDebugCode                 = CorDebugVersion_1_0,
        ver_ICorDebugClass                = CorDebugVersion_1_0,
        ver_ICorDebugEval                 = CorDebugVersion_1_0,
        ver_ICorDebugValue                = CorDebugVersion_1_0,
        ver_ICorDebugGenericValue         = CorDebugVersion_1_0,
        ver_ICorDebugReferenceValue       = CorDebugVersion_1_0,
        ver_ICorDebugHeapValue            = CorDebugVersion_1_0,
        ver_ICorDebugObjectValue          = CorDebugVersion_1_0,
        ver_ICorDebugBoxValue             = CorDebugVersion_1_0,
        ver_ICorDebugStringValue          = CorDebugVersion_1_0,
        ver_ICorDebugArrayValue           = CorDebugVersion_1_0,
        ver_ICorDebugContext              = CorDebugVersion_1_0,
        ver_ICorDebugEnum                 = CorDebugVersion_1_0,
        ver_ICorDebugObjectEnum           = CorDebugVersion_1_0,
        ver_ICorDebugBreakpointEnum       = CorDebugVersion_1_0,
        ver_ICorDebugStepperEnum          = CorDebugVersion_1_0,
        ver_ICorDebugProcessEnum          = CorDebugVersion_1_0,
        ver_ICorDebugThreadEnum           = CorDebugVersion_1_0,
        ver_ICorDebugFrameEnum            = CorDebugVersion_1_0,
        ver_ICorDebugChainEnum            = CorDebugVersion_1_0,
        ver_ICorDebugModuleEnum           = CorDebugVersion_1_0,
        ver_ICorDebugValueEnum            = CorDebugVersion_1_0,
        ver_ICorDebugCodeEnum             = CorDebugVersion_1_0,
        ver_ICorDebugTypeEnum             = CorDebugVersion_1_0,
        ver_ICorDebugErrorInfoEnum        = CorDebugVersion_1_0,
        ver_ICorDebugAppDomainEnum        = CorDebugVersion_1_0,
        ver_ICorDebugAssemblyEnum         = CorDebugVersion_1_0,
        ver_ICorDebugEditAndContinueErrorInfo 
                                          = CorDebugVersion_1_0,
        ver_ICorDebugEditAndContinueSnapshot 
                                          = CorDebugVersion_1_0,
        
        CorDebugVersion_1_1               = CorDebugVersion_1_0 + 1,
        // No interface definitions in version 1.1.
        
        CorDebugVersion_2_0 = CorDebugVersion_1_1 + 1,
        
        ver_ICorDebugManagedCallback2    = CorDebugVersion_2_0,
        ver_ICorDebugAppDomain2          = CorDebugVersion_2_0,
        ver_ICorDebugProcess2            = CorDebugVersion_2_0,
        ver_ICorDebugStepper2            = CorDebugVersion_2_0,
        ver_ICorDebugRegisterSet2        = CorDebugVersion_2_0,
        ver_ICorDebugThread2             = CorDebugVersion_2_0,
        ver_ICorDebugILFrame2            = CorDebugVersion_2_0,
        ver_ICorDebugModule2             = CorDebugVersion_2_0,
        ver_ICorDebugFunction2           = CorDebugVersion_2_0,
        ver_ICorDebugCode2               = CorDebugVersion_2_0,
        ver_ICorDebugClass2              = CorDebugVersion_2_0,
        ver_ICorDebugValue2              = CorDebugVersion_2_0,
        ver_ICorDebugEval2               = CorDebugVersion_2_0,
        ver_ICorDebugObjectValue2        = CorDebugVersion_2_0,
        
        CorDebugVersion_3_0 = CorDebugVersion_2_0 + 1,
        
        ver_ICorDebugThread3             = CorDebugVersion_3_0,
        ver_ICorDebugThread4             = CorDebugVersion_3_0,
        ver_ICorDebugStackWalk           = CorDebugVersion_3_0,
        ver_ICorDebugNativeFrame2        = CorDebugVersion_3_0,
        ver_ICorDebugInternalFrame2      = CorDebugVersion_3_0,
        ver_ICorDebugRuntimeUnwindableFrame = CorDebugVersion_3_0,
        ver_ICorDebugHeapValue3          = CorDebugVersion_3_0,
        ver_ICorDebugBlockingObjectEnum  = CorDebugVersion_3_0,
        
        CorDebugLatestVersion = CorDebugVersion_3_0
        
    }

    [Guid("ECCCCF2E-B286-4b3e-A983-860A8793D105")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebug2
    {
        [PreserveSig]int SetDebuggerVersion( CorDebugInterfaceVersion maxDebuggerVersion );
    }

    [Guid("3D6F5F60-7538-11D3-8D5B-00104B35E7EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugManagedCallback
    {
        [PreserveSig]int Breakpoint([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In]IntPtr pBreakpoint);
        [PreserveSig]int StepComplete([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugStepper pStepper, [In]CorDebugStepReason reason);
        [PreserveSig]int Break([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread thread);
        [PreserveSig]int Exception([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In]int unhandled);
        [PreserveSig]int EvalComplete([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugEval pEval);
        [PreserveSig]int EvalException([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugEval pEval);
        [PreserveSig]int CreateProcess([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess);
        [PreserveSig]int ExitProcess([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess);
        [PreserveSig]int CreateThread([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread thread);
        [PreserveSig]int ExitThread([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread thread);
        [PreserveSig]int LoadModule([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugModule pModule);
        [PreserveSig]int UnloadModule([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugModule pModule);
        [PreserveSig]int LoadClass([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass c);
        [PreserveSig]int UnloadClass([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass c);
        [PreserveSig]int DebuggerError([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In, MarshalAs(UnmanagedType.Error)]int errorHR, [In]uint errorCode);
        [PreserveSig]int LogMessage([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In]int lLevel, [In]string pLogSwitchName, [In]string pMessage);
        [PreserveSig]int LogSwitch([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In]int lLevel, [In]uint ulReason, [In]ref ushort pLogSwitchName, [In]ref ushort pParentName);
        [PreserveSig]int CreateAppDomain([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain);
        [PreserveSig]int ExitAppDomain([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain);
        [PreserveSig]int LoadAssembly([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugAssembly pAssembly);
        [PreserveSig]int UnloadAssembly([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugAssembly pAssembly);
        [PreserveSig]int ControlCTrap([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess);
        [PreserveSig]int NameChange([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread);
        [PreserveSig]int UpdateModuleSymbols([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugModule pModule, [In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.OLE.Interop.IStream pSymbolStream);
        [PreserveSig]int EditAndContinueRemap([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFunction pFunction, [In]int fAccurate);
        [PreserveSig]int BreakpointSetError([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugBreakpoint pBreakpoint, [In]uint dwError);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("5263E909-8CB5-11D3-BD2F-0000F80849BD")]
    [ComImport]
    public interface ICorDebugUnmanagedCallback
    {
        [PreserveSig]int DebugEvent([ComAliasName("CorDebugInterop.ULONG_PTR"), In]uint pDebugEvent, [In]int fOutOfBand);
    }

    [ComConversionLoss]
    public struct _STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public int lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [ComConversionLoss]
    public struct _PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    [Serializable]
    public enum CorDebugCreateProcessFlags
    {
        DEBUG_NO_SPECIAL_OPTIONS = 0,
    }

    [Guid("3D6F5F64-7538-11D3-8D5B-00104B35E7EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugProcess : ICorDebugController
    {
        //ICorDebugController
        [PreserveSig]int Stop([In]uint dwTimeout);
        [PreserveSig]int Continue([In]int fIsOutOfBand);
        [PreserveSig]int IsRunning([Out]out int pbRunning);
        [PreserveSig]int HasQueuedCallbacks([In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [Out]out int pbQueued);
        [PreserveSig]int EnumerateThreads([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThreadEnum ppThreads);
        [PreserveSig]int SetAllThreadsDebugState([In]CorDebugThreadState state, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pExceptThisThread);
        [PreserveSig]int Detach();
        [PreserveSig]int Terminate([In]uint exitCode);
        [PreserveSig]int CanCommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
        [PreserveSig]int CommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
        //ICorDebugProcess
        [PreserveSig]int GetID([Out]out uint pdwProcessId);
        [PreserveSig]int GetHandle([ComAliasName("CorDebugInterop.long"), Out]out uint phProcessHandle);
        [PreserveSig]int GetThread([In]uint dwThreadId, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThread ppThread);
        [PreserveSig]int EnumerateObjects([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugObjectEnum ppObjects);
        [PreserveSig]int IsTransitionStub([In]ulong address, [Out]out int pbTransitionStub);
        [PreserveSig]int IsOSSuspended([In]uint threadID, [Out]out int pbSuspended);
        [PreserveSig]int GetThreadContext([In]uint threadID, [In]uint contextSize, [Out]IntPtr context);
        [PreserveSig]int SetThreadContext([In]uint threadID, [In]uint contextSize, [In]IntPtr context);
        [PreserveSig]int ReadMemory([In]ulong address, [In]uint size, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]byte[] buffer, [Out]out uint read);
        [PreserveSig]int WriteMemory([In]ulong address, [In]uint size, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]byte[] buffer, [Out]out uint written);
        [PreserveSig]int ClearCurrentException([In]uint threadID);
        [PreserveSig]int EnableLogMessages([In]int fOnOff);
        [PreserveSig]int ModifyLogSwitch([In, MarshalAs(UnmanagedType.LPWStr)] string pLogSwitchName, [In]int lLevel);
        [PreserveSig]int EnumerateAppDomains([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAppDomainEnum ppAppDomains);
        [PreserveSig]int GetObject([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppObject);
        [PreserveSig]int ThreadForFiberCookie([In]uint fiberCookie, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThread ppThread);
        [PreserveSig]int GetHelperThreadID([Out]out uint pThreadID);
    }

    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCB05-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugProcessEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugProcessEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr processes, [Out]out uint pceltFetched);
    }

    [ComConversionLoss]
    [Guid("3D6F5F63-7538-11D3-8D5B-00104B35E7EF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugAppDomain : ICorDebugController
    {
        //ICorDebugController
        [PreserveSig]int Stop([In]uint dwTimeout);
        [PreserveSig]int Continue([In]int fIsOutOfBand);
        [PreserveSig]int IsRunning([Out]out int pbRunning);
        [PreserveSig]int HasQueuedCallbacks([In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [Out]out int pbQueued);
        [PreserveSig]int EnumerateThreads([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThreadEnum ppThreads);
        [PreserveSig]int SetAllThreadsDebugState([In]CorDebugThreadState state, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pExceptThisThread);
        [PreserveSig]int Detach();
        [PreserveSig]int Terminate([In]uint exitCode);
        [PreserveSig]int CanCommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
        [PreserveSig]int CommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
        //ICorDebugAppDomain
        [PreserveSig]int GetProcess([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int EnumerateAssemblies([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAssemblyEnum ppAssemblies);
        [PreserveSig]int GetModuleFromMetaDataInterface([In, MarshalAs(UnmanagedType.IUnknown)]object pIMetaData, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModule ppModule);
        [PreserveSig]int EnumerateBreakpoints([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugBreakpointEnum ppBreakpoints);
        [PreserveSig]int EnumerateSteppers([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepperEnum ppSteppers);
        [PreserveSig]int IsAttached([Out]out int pbAttached);
        [PreserveSig]int GetName([In]uint cchName, [Out]IntPtr pcchName, [Out]IntPtr szName);
        [PreserveSig]int GetObject([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppObject);
        [PreserveSig]int Attach();
        [PreserveSig]int GetID([Out]out uint pId);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("938C6D66-7FB6-4F69-B389-425B8987329B")]
    [ComImport]
    public interface ICorDebugThread
    {
        [PreserveSig]int GetProcess( [Out, MarshalAs( UnmanagedType.Interface )]out ICorDebugProcess ppProcess );
        [PreserveSig]int GetID([Out]out uint pdwThreadId);
        [PreserveSig]int GetHandle([ComAliasName("CorDebugInterop.long"), Out]out uint phThreadHandle);
        [PreserveSig]int GetAppDomain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAppDomain ppAppDomain);
        [PreserveSig]int SetDebugState([In]CorDebugThreadState state);
        [PreserveSig]int GetDebugState([Out]out CorDebugThreadState pState);
        [PreserveSig]int GetUserState([Out]out CorDebugUserState pState);
        [PreserveSig]int GetCurrentException([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppExceptionObject);
        [PreserveSig]int ClearCurrentException();
        [PreserveSig]int CreateStepper([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepper ppStepper);
        [PreserveSig]int EnumerateChains([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChainEnum ppChains);
        [PreserveSig]int GetActiveChain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetActiveFrame([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetRegisterSet([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugRegisterSet ppRegisters);
        [PreserveSig]int CreateEval([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugEval ppEval);
        [PreserveSig]int GetObject([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppObject);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAEC-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugStepper
    {
        [PreserveSig]int IsActive([Out]out int pbActive);
        [PreserveSig]int Deactivate();
        [PreserveSig]int SetInterceptMask([In]CorDebugIntercept mask);
        [PreserveSig]int SetUnmappedStopMask([In]CorDebugUnmappedStop mask);
        [PreserveSig]int Step([In]int bStepIn);
        [PreserveSig]int StepRange([In]int bStepIn, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]COR_DEBUG_STEP_RANGE[] ranges, [In]uint cRangeCount);
        [PreserveSig]int StepOut();
        [PreserveSig]int SetRangeIL([In]int bIL);
    }

    [Serializable]
    public enum CorDebugStepReason
    {
        STEP_NORMAL = 0,
        STEP_RETURN = 1,
        STEP_CALL = 2,
        STEP_EXCEPTION_FILTER = 3,
        STEP_EXCEPTION_HANDLER = 4,
        STEP_INTERCEPT = 5,
        STEP_EXIT = 6,
    }

    [Guid("CC7BCAF6-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugEval
    {
        [PreserveSig]int CallFunction([In, MarshalAs( UnmanagedType.Interface )]ICorDebugFunction pFunction, [In]uint nArgs, [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )]ICorDebugValue[] ppArgs);
        [PreserveSig]int NewObject([In, MarshalAs( UnmanagedType.Interface )]ICorDebugFunction pConstructor, [In]uint nArgs, [In, MarshalAs( UnmanagedType.LPArray, SizeParamIndex = 1 )]ICorDebugValue[] ppArgs);
        [PreserveSig]int NewObjectNoConstructor([In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass pClass);
        [PreserveSig]int NewString([In, MarshalAs(UnmanagedType.LPWStr)]string @string);
        [PreserveSig]int NewArray([In]CorElementType elementType, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass pElementClass, [In]uint rank, [In] ref uint dims, [In] ref uint lowBounds);
        [PreserveSig]int IsActive([Out]out int pbActive);
        [PreserveSig]int Abort();
        [PreserveSig]int GetResult([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppResult);
        [PreserveSig]int GetThread([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThread ppThread);
        [PreserveSig]int CreateValue([In]CorElementType elementType, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass pElementClass, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
    }

    [Guid("DBA2D8C1-E5C5-4069-8C13-10A7C6ABF43D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugModule
    {
        [PreserveSig]int GetProcess([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int GetBaseAddress([Out]out ulong pAddress);
        [PreserveSig]int GetAssembly([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAssembly ppAssembly);
        [PreserveSig]int GetName([In]uint cchName, [Out]IntPtr pcchName, [Out]IntPtr szName);
        [PreserveSig]int EnableJITDebugging([In]int bTrackJITInfo, [In]int bAllowJitOpts);
        [PreserveSig]int EnableClassLoadCallbacks([In]int bClassLoadCallbacks);
        [PreserveSig]int GetFunctionFromToken([In]uint methodDef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetFunctionFromRVA([In]ulong rva, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetClassFromToken([In]uint typeDef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugClass ppClass);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModuleBreakpoint ppBreakpoint);
        [PreserveSig]int GetEditAndContinueSnapshot([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugEditAndContinueSnapshot ppEditAndContinueSnapshot);
        [PreserveSig]int GetMetaDataInterface([In]ref Guid riid, [Out]out IntPtr ppObj);
        [PreserveSig]int GetToken([Out]out uint pToken);
        [PreserveSig]int IsDynamic([Out]out int pDynamic);
        [PreserveSig]int GetGlobalVariableValue([In]uint fieldDef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetSize([Out]out uint pcBytes);
        [PreserveSig]int IsInMemory([Out]out int pInMemory);
    }

    [Guid("CC7BCAF5-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugClass
    {
        [PreserveSig]int GetModule([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModule pModule);
        [PreserveSig]int GetToken([Out]out uint pTypeDef);
        [PreserveSig]int GetStaticFieldValue([In]uint fieldDef, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFrame pFrame, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
    }

    [ComConversionLoss]
    [Guid("DF59507C-D47A-459E-BCE2-6427EAC8FD06")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugAssembly
    {
        [PreserveSig]int GetProcess([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugProcess ppProcess);
        [PreserveSig]int GetAppDomain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAppDomain ppAppDomain);
        [PreserveSig]int EnumerateModules([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModuleEnum ppModules);
        [PreserveSig]int GetCodeBase([In]uint cchName, [Out]IntPtr pcchName, [Out]IntPtr szName);
        [PreserveSig]int GetName([In]uint cchName, [Out]IntPtr pcchName, [Out]IntPtr szName);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAF3-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugFunction
    {
        [PreserveSig]int GetModule([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModule ppModule);
        [PreserveSig]int GetClass([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugClass ppClass);
        [PreserveSig]int GetToken([Out]out uint pMethodDef);
        [PreserveSig]int GetILCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int GetNativeCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunctionBreakpoint ppBreakpoint);
        [PreserveSig]int GetLocalVarSigToken([Out]out uint pmdSig);
        [PreserveSig]int GetCurrentVersionNumber([Out]out uint pnCurrentVersion);
    }

    [Guid("CC7BCAE8-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugBreakpoint
    {
        [PreserveSig]int Activate([In]int bActive);
        [PreserveSig]int IsActive([Out]out int pbActive);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3D6F5F62-7538-11D3-8D5B-00104B35E7EF")]
    [ComImport]
    public interface ICorDebugController
    {
        [PreserveSig]int Stop([In]uint dwTimeout);
        [PreserveSig]int Continue([In]int fIsOutOfBand);
        [PreserveSig]int IsRunning([Out]out int pbRunning);
        [PreserveSig]int HasQueuedCallbacks([In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [Out]out int pbQueued);
        [PreserveSig]int EnumerateThreads([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThreadEnum ppThreads);
        [PreserveSig]int SetAllThreadsDebugState([In]CorDebugThreadState state, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pExceptThisThread);
        [PreserveSig]int Detach();
        [PreserveSig]int Terminate([In]uint exitCode);
        [PreserveSig]int CanCommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
        [PreserveSig]int CommitChanges([In]uint cSnapshots, [In, MarshalAs(UnmanagedType.Interface)]ref ICorDebugEditAndContinueSnapshot pSnapshots, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugErrorInfoEnum pError);
    }

    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCB06-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugThreadEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugThreadEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr threads, [Out]out uint pceltFetched);
    }

    [Serializable]
    public enum CorDebugThreadState
    {
        THREAD_RUN = 0,
        THREAD_SUSPEND = 1,
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6DC3FA01-D7CB-11D2-8A95-0080C792E5D8")]
    [ComImport]
    public interface ICorDebugEditAndContinueSnapshot
    {
        [PreserveSig]int CopyMetaData([In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.OLE.Interop.IStream pIStream, [Out]out Guid pMvid);
        [PreserveSig]int GetMvid([Out]out Guid pMvid);
        [PreserveSig]int GetRoDataRVA([Out]out uint pRoDataRVA);
        [PreserveSig]int GetRwDataRVA([Out]out uint pRwDataRVA);
        [PreserveSig]int SetPEBytes([In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.OLE.Interop.IStream pIStream);
        [PreserveSig]int SetILMap([In]uint mdFunction, [In]uint cMapSize, [In]ref _COR_IL_MAP map);
        [PreserveSig]int SetPESymbolBytes([In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.OLE.Interop.IStream pIStream);
    }

    [ComConversionLoss]
    [Guid("F0E18809-72B5-11D2-976F-00A0C9B4D50C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugErrorInfoEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugErrorInfoEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr errors, [Out]out uint pceltFetched);
    }

    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("4A2A1EC9-85EC-4BFB-9F15-A89FDFE0FE83")]
    [ComImport]
    public interface ICorDebugAssemblyEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugAssemblyEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr values, [Out]out uint pceltFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [Guid("CC7BCB03-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugBreakpointEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugBreakpointEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr breakpoints, [Out]out uint pceltFetched);
    }

    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCB04-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugStepperEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugStepperEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr steppers, [Out]out uint pceltFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAF7-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugValue
    {
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
    }

    [Serializable]
    public enum CorDebugUserState
    {
        USER_STOP_REQUESTED = 0x1,
        USER_SUSPEND_REQUESTED = 0x2,
        USER_BACKGROUND = 0x4,
        USER_UNSTARTED = 0x8,
        USER_STOPPED = 0x10,
        USER_WAIT_SLEEP_JOIN = 0x20,
        USER_SUSPENDED = 0x40,
        USER_UNSAFE_POINT = 0x80,
    }

    [Guid("CC7BCB08-8A68-11D2-983C-0000F808342D")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugChainEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugChainEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr chains, [Out]out uint pceltFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAEE-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugChain
    {
        [PreserveSig]int GetThread([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThread ppThread);
        [PreserveSig]int GetStackRange([Out]out ulong pStart, [Out]out ulong pEnd);
        [PreserveSig]int GetContext([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugContext ppContext);
        [PreserveSig]int GetCaller([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetCallee([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetPrevious([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetNext([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int IsManaged([Out]out int pManaged);
        [PreserveSig]int EnumerateFrames([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrameEnum ppFrames);
        [PreserveSig]int GetActiveFrame([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetRegisterSet([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugRegisterSet ppRegisters);
        [PreserveSig]int GetReason([Out]out CorDebugChainReason pReason);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAEF-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugFrame
    {
        [PreserveSig]int GetChain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetFunctionToken([Out]out uint pToken);
        [PreserveSig]int GetStackRange([Out]out ulong pStart, [Out]out ulong pEnd);
        [PreserveSig]int GetCaller([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetCallee([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int CreateStepper([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepper ppStepper);
    }

    public enum CorDebugRegister
    {
        REGISTER_INSTRUCTION_POINTER = 0,
        REGISTER_STACK_POINTER,
        REGISTER_FRAME_POINTER,
        
        REGISTER_X86_EIP = 0,
        REGISTER_X86_ESP,
        REGISTER_X86_EBP,
        REGISTER_X86_EAX,
        REGISTER_X86_ECX,
        REGISTER_X86_EDX,
        REGISTER_X86_EBX,
        REGISTER_X86_ESI,
        REGISTER_X86_EDI,
        REGISTER_X86_FPSTACK_0,
        REGISTER_X86_FPSTACK_1,
        REGISTER_X86_FPSTACK_2,
        REGISTER_X86_FPSTACK_3,
        REGISTER_X86_FPSTACK_4,
        REGISTER_X86_FPSTACK_5,
        REGISTER_X86_FPSTACK_6,
        REGISTER_X86_FPSTACK_7,
        
        REGISTER_AMD64_RIP = 0,
        REGISTER_AMD64_RSP,
        REGISTER_AMD64_RBP,
        REGISTER_AMD64_RAX,
        REGISTER_AMD64_RCX,
        REGISTER_AMD64_RDX,
        REGISTER_AMD64_RBX,       
        REGISTER_AMD64_RSI,
        REGISTER_AMD64_RDI,
        REGISTER_AMD64_R8,
        REGISTER_AMD64_R9,
        REGISTER_AMD64_R10,
        REGISTER_AMD64_R11,
        REGISTER_AMD64_R12,
        REGISTER_AMD64_R13,
        REGISTER_AMD64_R14,
        REGISTER_AMD64_R15,        
        REGISTER_AMD64_XMM0,
        REGISTER_AMD64_XMM1,
        REGISTER_AMD64_XMM2,
        REGISTER_AMD64_XMM3,
        REGISTER_AMD64_XMM4,
        REGISTER_AMD64_XMM5,
        REGISTER_AMD64_XMM6,
        REGISTER_AMD64_XMM7,
        REGISTER_AMD64_XMM8,
        REGISTER_AMD64_XMM9,
        REGISTER_AMD64_XMM10,
        REGISTER_AMD64_XMM11,
        REGISTER_AMD64_XMM12,
        REGISTER_AMD64_XMM13,
        REGISTER_AMD64_XMM14,
        REGISTER_AMD64_XMM15,

        REGISTER_IA64_BSP = REGISTER_FRAME_POINTER,
        REGISTER_IA64_R0  = REGISTER_IA64_BSP + 1,
        REGISTER_IA64_F0  = REGISTER_IA64_R0  + 128,
    } 

    [Guid("CC7BCB0B-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugRegisterSet
    {
        [PreserveSig]int GetRegistersAvailable([Out]out ulong pAvailable);
        [PreserveSig]int GetRegisters([In]ulong mask, [In]uint regCount, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)]ulong[] regBuffer);
        [PreserveSig]int SetRegisters([In]ulong mask, [In]uint regCount, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)]ulong[] regBuffer);
        [PreserveSig]int GetThreadContext([In]uint contextSize, [Out]IntPtr context);
        [PreserveSig]int SetThreadContext([In]uint contextSize, [In]IntPtr context);
    }

    [Guid("6DC7BA3F-89BA-4459-9EC1-9D60937B468D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]    
    public interface ICorDebugRegisterSet2
    {
        [PreserveSig]int GetRegistersAvailable([In] uint numChunks, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]byte[] availableRegChunks);
        [PreserveSig]int GetRegisters([In] uint maskCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] mask, [In] uint regCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] ulong[] regBuffer);
        [PreserveSig]int SetRegisters([In] uint maskCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] byte[] mask, [In] uint regCount, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] ulong[] regBuffer);
    }

    [Guid("CC7BCB02-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugObjectEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugObjectEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr objects, [Out]out uint pceltFetched);
    }

    [Guid("63CA1B24-4359-4883-BD57-13F815F58744")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugAppDomainEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugAppDomainEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr values, [Out]out uint pceltFetched);
    }

    [Guid("CC7BCB01-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugEnum
    {
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAEB-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugValueBreakpoint : ICorDebugBreakpoint
    {
        //ICorDebugBreakpoint
        [PreserveSig]int Activate([In]int bActive);
        [PreserveSig]int IsActive([Out]out int pbActive);
        //ICorDebugValueBreakpoint
        [PreserveSig]int GetValue([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
    }

    [Serializable]
    public enum CorDebugIntercept
    {
        INTERCEPT_NONE = 0x0,
        INTERCEPT_CLASS_INIT = 0x1,
        INTERCEPT_EXCEPTION_FILTER = 0x2,
        INTERCEPT_SECURITY = 0x4,
        INTERCEPT_CONTEXT_POLICY = 0x8,
        INTERCEPT_INTERCEPTION = 0x10,
        INTERCEPT_ALL = 0xffff,
    }

    [Serializable]
    public enum CorDebugUnmappedStop
    {
        STOP_NONE = 0x0,
        STOP_PROLOG = 0x1,
        STOP_EPILOG = 0x2,
        STOP_NO_MAPPING_INFO = 0x4,
        STOP_OTHER_UNMAPPED = 0x8,
        STOP_UNMANAGED = 0x10,
        STOP_ALL = 0xffff,
        STOP_ONLYJUSTMYCODE = 0x10000,
    }

    public struct COR_DEBUG_STEP_RANGE
    {
        public uint startOffset;
        public uint endOffset;
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCB00-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugContext : ICorDebugObjectValue
    {
        //ICorDebugObjectValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        [PreserveSig]int GetClass([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugClass ppClass);
        [PreserveSig]int GetFieldValue([In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass pClass, [In]uint fieldDef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetVirtualMethod([In]uint memberRef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetContext([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugContext ppContext);
        [PreserveSig]int IsValueClass([Out]out int pbIsValueClass);
        [PreserveSig]int GetManagedCopy([Out, MarshalAs(UnmanagedType.IUnknown)]out object ppObject);
        [PreserveSig]int SetFromManagedCopy([In, MarshalAs(UnmanagedType.IUnknown)]object pObject);
        //ICorDebugContext
    }

    [Guid("CC7BCB07-8A68-11D2-983C-0000F808342D")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugFrameEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugFrameEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr frames, [Out]out uint pceltFetched);
    }

    [Serializable]
    public enum CorDebugChainReason
    {
        CHAIN_NONE = 0x0,
        CHAIN_CLASS_INIT = 0x1,
        CHAIN_EXCEPTION_FILTER = 0x2,
        CHAIN_SECURITY = 0x4,
        CHAIN_CONTEXT_POLICY = 0x8,
        CHAIN_INTERCEPTION = 0x10,
        CHAIN_PROCESS_START = 0x20,
        CHAIN_THREAD_START = 0x40,
        CHAIN_ENTER_MANAGED = 0x80,
        CHAIN_ENTER_UNMANAGED = 0x100,
        CHAIN_DEBUGGER_EVAL = 0x200,
        CHAIN_CONTEXT_SWITCH = 0x400,
        CHAIN_FUNC_EVAL = 0x800,
    }

    [Guid("18AD3D6E-B7D2-11D2-BD04-0000F80849BD")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugObjectValue : ICorDebugValue
    {
        //ICorDebugValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugObjectValue
        [PreserveSig]int GetClass([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugClass ppClass);
        [PreserveSig]int GetFieldValue([In, MarshalAs(UnmanagedType.Interface)]ICorDebugClass pClass, [In]uint fieldDef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetVirtualMethod([In]uint memberRef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetContext([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugContext ppContext);
        [PreserveSig]int IsValueClass([Out]out int pbIsValueClass);
        [PreserveSig]int GetManagedCopy([Out, MarshalAs(UnmanagedType.IUnknown)]out object ppObject);
        [PreserveSig]int SetFromManagedCopy([In, MarshalAs(UnmanagedType.IUnknown)]object pObject);        
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAEA-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugModuleBreakpoint : ICorDebugBreakpoint
    {
        //ICorDebugBreakpoint
        [PreserveSig]int Activate([In]int bActive);
        [PreserveSig]int IsActive([Out]out int pbActive);
        //ICorDebugModuleBreakpoint
        [PreserveSig]int GetModule([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugModule ppModule);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCB09-8A68-11D2-983C-0000F808342D")]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugModuleEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugModuleEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr modules, [Out]out uint pceltFetched);
    }

    [Guid("CC7BCAF4-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugCode
    {
        [PreserveSig]int IsIL([Out]out int pbIL);
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetAddress([Out]out ulong pStart);
        [PreserveSig]int GetSize([Out]out uint pcBytes);
        [PreserveSig]int CreateBreakpoint([In]uint offset, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunctionBreakpoint ppBreakpoint);
        [PreserveSig]int GetCode([In]uint startOffset, [In]uint endOffset, [In]uint cBufferAlloc, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=2)]byte[] buffer, [Out]out uint pcBufferSize);
        [PreserveSig]int GetVersionNumber([Out]out uint nVersion);
        [PreserveSig]int GetILToNativeMapping([In]uint cMap, [Out]out uint pcMap, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] COR_DEBUG_IL_TO_NATIVE_MAP[] map);
        [PreserveSig]int GetEnCRemapSequencePoints([In]uint cMap, [Out]out uint pcMap, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]uint[] offsets);
    }

    public struct CodeChunkInfo
    {
        public ulong startAddr;
        public uint length;
    }

    public struct COR_DEBUG_IL_TO_NATIVE_MAP
    {
        public uint ilOffset;
        public uint nativeStartOffset;
        public uint nativeEndOffset;
    }

    enum CorDebugIlToNativeMappingTypes : uint
    {
        NO_MAPPING = unchecked((uint)-1),
        PROLOG = unchecked((uint)-2),
        EPILOG = unchecked((uint)-3 )
    }

    [Guid("5F696509-452F-4436-A3FE-4D11FE7E2347")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugCode2
    {
        [PreserveSig]int GetCodeChunks([In] uint cbufSize, [Out]out uint pcnumChunks, [In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)]CodeChunkInfo[] chunks);
        [PreserveSig]int GetCompilerFlags( [Out] out int pdwFlags );
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAE9-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugFunctionBreakpoint : ICorDebugBreakpoint
    {
        //ICorDebugBreakpoint
        [PreserveSig]int Activate([In]int bActive);
        [PreserveSig]int IsActive([Out]out int pbActive);
        //ICorDebugFunctionBreakpoint
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetOffset([Out]out uint pnOffset);
    }

    public struct _COR_IL_MAP
    {
        public uint oldOffset;
        public uint newOffset;
        public int fAccurate;
    }

    public struct _SECURITY_ATTRIBUTES
    {
        public uint nLength;
        public IntPtr lpSecurityDescriptor;
        public int bInheritHandle;
    }

    [Guid("CC7BCAF9-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugReferenceValue : ICorDebugValue
    {
        //ICorDebugValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugReferenceValue
        [PreserveSig]int IsNull([Out]out int pbNull);
        [PreserveSig]int GetValue([Out]out ulong pValue);
        [PreserveSig]int SetValue([In]ulong value);
        [PreserveSig]int Dereference([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int DereferenceStrong([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
    }

    [Guid("CC7BCAFA-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugHeapValue : ICorDebugValue
    {
        //ICorDebugValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugHeapValue
        [PreserveSig]int IsValid([Out]out int pbValid);
        [PreserveSig]int CreateRelocBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
    }

    [Guid("CC7BCAFD-8A68-11D2-983C-0000F808342D")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugStringValue : ICorDebugHeapValue
    {
        //ICorDebugHeapValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        [PreserveSig]int IsValid([Out]out int pbValid);
        [PreserveSig]int CreateRelocBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugStringValue
        [PreserveSig]int GetLength([Out]out uint pcchString);
        [PreserveSig]int GetString([In]uint cchString, [Out]IntPtr pcchString, [Out] IntPtr szString);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC7BCAF8-8A68-11D2-983C-0000F808342D")]
    [ComImport]
    public interface ICorDebugGenericValue : ICorDebugValue
    {
        //ICorDebugValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugGenericValue
        [PreserveSig]int GetValue([Out]IntPtr pTo);
        [PreserveSig]int SetValue([In]IntPtr pFrom);
    }

    [Guid("CC7BCAFC-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugBoxValue : ICorDebugHeapValue
    {
        //ICorDebugHeapValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        [PreserveSig]int IsValid([Out]out int pbValid);
        [PreserveSig]int CreateRelocBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugBoxValue
        [PreserveSig]int GetObject([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugObjectValue ppObject);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("0405B0DF-A660-11D2-BD02-0000F80849BD")]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugArrayValue : ICorDebugHeapValue
    {
        //ICorDebugHeapValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        [PreserveSig]int IsValid([Out]out int pbValid);
        [PreserveSig]int CreateRelocBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        //ICorDebugArrayValue
        [PreserveSig]int GetElementType([Out]out CorElementType pType);
        [PreserveSig]int GetRank([Out]out uint pnRank);
        [PreserveSig]int GetCount([Out]out uint pnCount);
        [PreserveSig]int GetDimensions([In]uint cdim, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]uint[] dims);
        [PreserveSig]int HasBaseIndicies([Out]out int pbHasBaseIndicies);
        [PreserveSig]int GetBaseIndicies([In]uint cdim, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]uint[] indicies);
        [PreserveSig]int GetElement([In]uint cdim, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]uint[] indices, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetElementAtPosition([In]uint nPosition, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
    }

    [Guid("03E26311-4F76-11D3-88C6-006097945418")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugILFrame : ICorDebugFrame
    {
        //ICorDebugFrame
        [PreserveSig]int GetChain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetFunctionToken([Out]out uint pToken);
        [PreserveSig]int GetStackRange([Out]out ulong pStart, [Out]out ulong pEnd);
        [PreserveSig]int GetCaller([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetCallee([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int CreateStepper([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepper ppStepper);
        //ICorDebugILFrame
        [PreserveSig]int GetIP([Out]out uint pnOffset, [Out]out CorDebugMappingResult pMappingResult);
        [PreserveSig]int SetIP([In]uint nOffset);
        [PreserveSig]int EnumerateLocalVariables([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueEnum ppValueEnum);
        [PreserveSig]int GetLocalVariable([In]uint dwIndex, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int EnumerateArguments([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueEnum ppValueEnum);
        [PreserveSig]int GetArgument([In]uint dwIndex, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetStackDepth([Out]out uint pDepth);
        [PreserveSig]int GetStackValue([In]uint dwIndex, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int CanSetIP([In]uint nOffset);
    }

    [Serializable]
    public enum CorDebugInternalFrameType
    {
        STUBFRAME_NONE = 0x00000000,
        STUBFRAME_M2U = 0x0000001,
        STUBFRAME_U2M = 0x0000002,
        STUBFRAME_APPDOMAIN_TRANSITION = 0x00000003,
        STUBFRAME_LIGHTWEIGHT_FUNCTION = 0x00000004,
        STUBFRAME_FUNC_EVAL = 0x00000005,
    }

    [Guid("B92CC7F7-9D2D-45c4-BC2B-621FCC9DFBF4")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugInternalFrame : ICorDebugFrame
    {
        //ICorDebugFrame     
        [PreserveSig]int GetChain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetFunctionToken([Out]out uint pToken);
        [PreserveSig]int GetStackRange([Out]out ulong pStart, [Out]out ulong pEnd);
        [PreserveSig]int GetCaller([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetCallee([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int CreateStepper([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepper ppStepper);
        //ICorDebugInternalFrame
        [PreserveSig]int GetFrameType([Out]out CorDebugInternalFrameType pType);
    }

    [Guid("03E26314-4F76-11d3-88C6-006097945418")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugNativeFrame : ICorDebugFrame
    {
        //Not yet verified to be correct interop definition
        //ICorDebugFrame                
        [PreserveSig]int GetChain([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugChain ppChain);
        [PreserveSig]int GetCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCode ppCode);
        [PreserveSig]int GetFunction([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction);
        [PreserveSig]int GetFunctionToken([Out]out uint pToken);
        [PreserveSig]int GetStackRange([Out]out ulong pStart, [Out]out ulong pEnd);
        [PreserveSig]int GetCaller([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int GetCallee([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFrame ppFrame);
        [PreserveSig]int CreateStepper([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugStepper ppStepper);
        //ICorDebugNativeFrame
        [PreserveSig]int GetIP([Out]out uint pnOffset);
        [PreserveSig]int SetIP([In] uint nOffset);
        [PreserveSig]int GetRegisterSet([Out] out ICorDebugRegisterSet ppRegisters);
        [PreserveSig]int GetLocalRegisterValue([In] CorDebugRegister reg, [In] uint cbSigBlob, [In] IntPtr pvSigBlob, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);
        [PreserveSig]int GetLocalDoubleRegisterValue([In] CorDebugRegister highWordReg, [In] CorDebugRegister lowWordReg, [In] uint cbSigBlob, [In] IntPtr pvSigBlob, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);
        [PreserveSig]int GetLocalMemoryValue([In] ulong address, [In] uint cbSigBlob, [In] IntPtr pvSigBlob, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);
        [PreserveSig]int GetLocalRegisterMemoryValue([In] CorDebugRegister highWordReg, [In] ulong lowWordAddress, [In] uint cbSigBlob, [In] IntPtr pvSigBlob, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);
        [PreserveSig]int GetLocalMemoryRegisterValue([In] ulong highWordAddress, [In] CorDebugRegister lowWordRegister, [In] uint cbSigBlob, [In] IntPtr pvSigBlob, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);
        [PreserveSig]int CanSetIP([In] uint nOffset);
    }

    [Serializable]
    public enum CorDebugMappingResult
    {
        MAPPING_PROLOG = 0x1,
        MAPPING_EPILOG = 0x2,
        MAPPING_NO_INFO = 0x4,
        MAPPING_UNMAPPED_ADDRESS = 0x8,
        MAPPING_EXACT = 0x10,
        MAPPING_APPROXIMATE = 0x20,
    }

    [Guid("CC7BCB0A-8A68-11D2-983C-0000F808342D")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComConversionLoss]
    [ComImport]
    public interface ICorDebugValueEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugValueEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr values, [Out]out uint pceltFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("250E5EEA-DB5C-4C76-B6F3-8C46F12E3203")]
    [ComImport]
    public interface ICorDebugManagedCallback2
    {
        [PreserveSig]int FunctionRemapOpportunity([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFunction pOldFunction, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFunction pNewFunction, [In]uint oldILOffset);
        [PreserveSig]int CreateConnection([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In]uint dwConnectionId, [In]ref ushort pConnName);
        [PreserveSig]int ChangeConnection([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In]uint dwConnectionId);
        [PreserveSig]int DestroyConnection([In, MarshalAs(UnmanagedType.Interface)]ICorDebugProcess pProcess, [In]uint dwConnectionId);
        [PreserveSig]int Exception([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFrame pFrame, [In]uint nOffset, [In]CorDebugExceptionCallbackType dwEventType, [In]uint dwFlags);
        [PreserveSig]int ExceptionUnwind([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In]CorDebugExceptionUnwindCallbackType dwEventType, [In]uint dwFlags);
        [PreserveSig]int FunctionRemapComplete([In, MarshalAs(UnmanagedType.Interface)]ICorDebugAppDomain pAppDomain, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFunction pFunction);
        [PreserveSig]int MDANotification([In, MarshalAs(UnmanagedType.Interface)]ICorDebugController pController, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugThread pThread, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugMDA pMDA);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("CC726F2F-1DB7-459b-B0EC-05F01D841B42")]
    [ComImport]
    public interface ICorDebugMDA
    {
        [PreserveSig]int GetName([In]uint cchName, [Out]IntPtr pcchName, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);
        [PreserveSig]int GetDescription([In]uint cchName, [Out]IntPtr pcchName, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);
        [PreserveSig]int GetXML([In]uint cchName, [Out]IntPtr pcchName, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] char[] szName);                
        [PreserveSig]int GetFlags([In]CorDebugMDAFlags pFlags);        
        [PreserveSig]int GetOSThreadId([Out]out uint pOsTid);
    }
    
    [Serializable]
    public enum CorDebugMDAFlags
    {	
        MDA_FLAG_SEVERE	= 0x1,
	MDA_FLAG_SLIP	= 0x2
    }

    [Serializable]
    public enum CorDebugExceptionCallbackType
    {
        DEBUG_EXCEPTION_FIRST_CHANCE = 1,
        DEBUG_EXCEPTION_USER_FIRST_CHANCE = 2,
        DEBUG_EXCEPTION_CATCH_HANDLER_FOUND = 3,
        DEBUG_EXCEPTION_UNHANDLED = 4,
    }

    [Serializable]
    public enum CorDebugExceptionUnwindCallbackType
    {
        DEBUG_EXCEPTION_UNWIND_BEGIN = 1,
        DEBUG_EXCEPTION_INTERCEPTED = 2,
    }

    [Guid("096E81D5-ECDA-4202-83F5-C65980A9EF75")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugAppDomain2
    {
        [PreserveSig]int GetArrayOrPointerType([In]CorElementType elementType, [In]uint nRank, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugType pTypeArg, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType ppType);
        [PreserveSig]int GetFunctionPointerType([In]uint nTypeArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] ICorDebugType[] ppTypeArgs, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType ppType);
    }

    [Guid("D613F0BB-ACE1-4C19-BD72-E4C08D5DA7F5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugType
    {
        [PreserveSig]int GetType([Out]out CorElementType ty);
        [PreserveSig]int GetClass([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugClass ppClass);
        [PreserveSig]int EnumerateTypeParameters([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugTypeEnum ppTyParEnum);
        [PreserveSig]int GetFirstTypeParameter([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType value);
        [PreserveSig]int GetBase([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType pBase);
        [PreserveSig]int GetStaticFieldValue([In]uint fieldDef, [In, MarshalAs(UnmanagedType.Interface)]ICorDebugFrame pFrame, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int GetRank([Out]out uint pnRank);
    }

    [Guid("10F27499-9DF2-43CE-8333-A321D7C99CB4")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugTypeEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugTypeEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr values, [Out]out uint pceltFetched);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct COR_ACTIVE_FUNCTION
    {
        public ICorDebugAppDomain pAppDomain;
        public ICorDebugModule pModule;
        public ICorDebugFunction2 pFunction;
        public uint ilOffset;
        public uint Flags;
    }

    [ComConversionLoss]
    [Guid("AD1B3588-0EF0-4744-A496-AA09A9F80371")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugProcess2
    {
        [PreserveSig]int GetThreadForTaskID([In]ulong taskid, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugThread2 ppThread);
        [PreserveSig]int GetVersion([Out]out _COR_VERSION version);
        [PreserveSig]int SetUnmanagedBreakpoint([In]ulong address, [In]uint bufsize, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] byte[] buffer, [Out]out uint bufLen);
        [PreserveSig]int ClearUnmanagedBreakpoint([In]ulong address);
        [PreserveSig]int SetDesiredNGENCompilerFlags([In]uint pdwFlags);
        [PreserveSig]int GetDesiredNGENCompilerFlags([Out]out uint pdwFlags);
        [PreserveSig]int GetReferenceValueFromGCHandle([In]UIntPtr handle, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugReferenceValue pOutValue);
    }

    [Guid("2BD956D9-7B07-4BEF-8A98-12AA862417C5")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugThread2
    {
        [PreserveSig]int GetActiveFunctions([In]uint cFunctions, [Out]out uint pcFunctions, [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] COR_ACTIVE_FUNCTION[] pFunctions);
        [PreserveSig]int GetConnectionID([Out]out uint pdwConnectionId);
        [PreserveSig]int GetTaskID([Out]out ulong pTaskId);
        [PreserveSig]int GetVolatileOSThreadID([Out]out uint pdwTid);
        [PreserveSig]int InterceptCurrentException([In, MarshalAs(UnmanagedType.Interface)]ICorDebugFrame pFrame);
    }

    public struct _COR_VERSION
    {
        public uint dwMajor;
        public uint dwMinor;
        public uint dwBuild;
        public uint dwSubBuild;
    }

    [Guid("C5B6E9C3-E7D1-4A8E-873B-7F047F0706F7")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugStepper2
    {
        [PreserveSig]int SetJMC([In]int fIsJMCStepper);
    }

    [Guid("5D88A994-6C30-479B-890F-BCEF88B129A5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugILFrame2
    {
        [PreserveSig]int RemapFunction([In]uint newILOffset);
        [PreserveSig]int EnumerateTypeParameters([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugTypeEnum ppTyParEnum);
    }

    public enum CorDebugJITCompilerFlags : uint
    {
        CORDEBUG_JIT_DEFAULT              = 0x1,                    		// Track info
        CORDEBUG_JIT_DISABLE_OPTIMIZATION = 0x3,     // Includes track info
        CORDEBUG_JIT_ENABLE_ENC           = 0x7                		// Includes track & disable opt
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("7FCC5FB5-49C0-41DE-9938-3B88B5B9ADD7")]
    [ComImport]
    public interface ICorDebugModule2
    {
        [PreserveSig]int SetJMCStatus([In]int bIsJustMyCode, [In]uint cTokens, [In]ref uint pTokens);
        [PreserveSig]int ApplyChanges([In]uint cbMetadata, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)]byte[] pbMetadata, [In]uint cbIL, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)]byte[] pbIL);
        [PreserveSig]int SetJITCompilerFlags([In]uint dwFlags);
        [PreserveSig]int GetJITCompilerFlags([Out]out uint pdwFlags);
        [PreserveSig]int ResolveAssembly([In]uint tkAssemblyRef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugAssembly ppAssembly);
    }

    [Guid("EF0C490B-94C3-4E4D-B629-DDC134C532D8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugFunction2
    {
        [PreserveSig]int SetJMCStatus([In]int bIsJustMyCode);
        [PreserveSig]int GetJMCStatus([Out]out int pbIsJustMyCode);
        [PreserveSig]int EnumerateNativeCode([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugCodeEnum ppCodeEnum);
        [PreserveSig]int GetVersionNumber([Out]out uint pnVersion);
    }

    [Guid("55E96461-9645-45E4-A2FF-0367877ABCDE")]
    [ComConversionLoss]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugCodeEnum : ICorDebugEnum
    {
        //ICorDebugEnum
        [PreserveSig]int Skip([In]uint celt);
        [PreserveSig]int Reset();
        [PreserveSig]int Clone([Out]out IntPtr ppEnum);
        [PreserveSig]int GetCount([Out]out uint pcelt);
        //ICorDebugCodeEnum
        [PreserveSig]int Next([In]uint celt, [Out]IntPtr values, [Out]out uint pceltFetched);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("B008EA8D-7AB1-43F7-BB20-FBB5A04038AE")]
    [ComImport]
    public interface ICorDebugClass2
    {
        [PreserveSig]int GetParameterizedType([In]CorElementType elementType, [In]uint nTypeArgs, [In, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex=1)] ICorDebugType[] ppTypeArgs, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType ppType);
        [PreserveSig]int SetJMCStatus([In]int bIsJustMyCode);
    }

    [Guid("FB0D9CE7-BE66-4683-9D32-A42A04E2FD91")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugEval2
    {
        [PreserveSig]int CallParameterizedFunction([In, MarshalAs(UnmanagedType.Interface)] ICorDebugFunction pFunction, [In] uint nTypeArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ICorDebugType[] ppTypeArgs, [In] uint nArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ICorDebugValue[] ppArgs);                
        [PreserveSig]int CreateValueForType([In, MarshalAs(UnmanagedType.Interface)] ICorDebugType pType, [Out, MarshalAs(UnmanagedType.Interface)] out ICorDebugValue ppValue);                
        [PreserveSig]int NewParameterizedObject([In, MarshalAs(UnmanagedType.Interface)] ICorDebugFunction pConstructor, [In] uint nTypeArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ICorDebugType[] ppTypeArgs, [In] uint nArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] ICorDebugValue[] ppArgs);                
        [PreserveSig]int NewParameterizedObjectNoConstructor([In, MarshalAs(UnmanagedType.Interface)] ICorDebugClass pClass, [In] uint nTypeArgs, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] ICorDebugType[] ppTypeArgs);                
        [PreserveSig]int NewParameterizedArray([In, MarshalAs(UnmanagedType.Interface)] ICorDebugType pElementType, [In] uint rank, [In] ref uint dims, [In] ref uint lowBounds);                
        [PreserveSig]int NewStringWithLength([In, MarshalAs(UnmanagedType.LPWStr)] string @string, [In] uint uiLength);                
        [PreserveSig]int RudeAbort();    
    }
    
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("5E0B54E7-D88A-4626-9420-A691E0A78B49")]
    [ComImport]
    public interface ICorDebugValue2
    {
        [PreserveSig]int GetExactType([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType ppType);
    }

    [Guid("49E4A320-4A9B-4ECA-B105-229FB7D5009F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugObjectValue2
    {
        [PreserveSig]int GetVirtualMethodAndType([In]uint memberRef, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugFunction ppFunction, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugType ppType);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("029596E8-276B-46A1-9821-732E96BBB00B")]
    [ComImport]
    public interface ICorDebugHandleValue : ICorDebugReferenceValue
    {
        //ICorDebugReferenceValue
        [PreserveSig]int GetType([Out]out CorElementType pType);
        [PreserveSig]int GetSize([Out]out uint pSize);
        [PreserveSig]int GetAddress([Out]out ulong pAddress);
        [PreserveSig]int CreateBreakpoint([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValueBreakpoint ppBreakpoint);
        [PreserveSig]int IsNull([Out]out int pbNull);
        [PreserveSig]int GetValue([Out]out ulong pValue);
        [PreserveSig]int SetValue([In]ulong value);
        [PreserveSig]int Dereference([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        [PreserveSig]int DereferenceStrong([Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugValue ppValue);
        //ICorDebugHandleValue
        [PreserveSig]int GetHandleType([Out]out CorDebugHandleType pType);
        [PreserveSig]int Dispose();
    }

    [Serializable]
    public enum CorDebugHandleType
    {
        HANDLE_STRONG = 1,
        HANDLE_WEAK_TRACK_RESURRECTION = 2,
    }

    [Guid("E3AC4D6C-9CB7-43E6-96CC-B21540E5083C")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface ICorDebugHeapValue2
    {
        [PreserveSig]int CreateHandle([In]CorDebugHandleType type, [Out, MarshalAs(UnmanagedType.Interface)]out ICorDebugHandleValue ppHandle);
    }

    [Guid("83C91210-A34F-427C-B35F-79C3995B3C14")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IDebugRemoteCorDebug
    {
        [PreserveSig]int CreateProcessEx([In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.Debugger.Interop.IDebugPort2 pPort, [In, MarshalAs(UnmanagedType.LPWStr)]string lpApplicationName, [In, MarshalAs(UnmanagedType.LPWStr)]string lpCommandLine, [In]IntPtr lpProcessAttributes, [In]IntPtr lpThreadAttributes, [In]int bInheritHandles, [In]uint dwCreationFlags, [In]IntPtr lpEnvironment, [In, MarshalAs(UnmanagedType.LPWStr)]string lpCurrentDirectory, [In]ref _STARTUPINFO lpStartupInfo, [In]ref _PROCESS_INFORMATION lpProcessInformation, [In]uint debuggingFlags, [Out, MarshalAs(UnmanagedType.IUnknown)]out object ppProcess);
        [PreserveSig]int DebugActiveProcessEx([In, MarshalAs(UnmanagedType.Interface)]Microsoft.VisualStudio.Debugger.Interop.IDebugPort2 pPort, [In]uint id, [In]int win32Attach, [Out, MarshalAs(UnmanagedType.IUnknown)]out object ppProcess);
    }

    [Guid("782CB503-84B1-4B8F-9AAD-A12B75905015")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [ComImport]
    public interface IVbCompilerHost
    {
        [PreserveSig]int OutputString([In, MarshalAs(UnmanagedType.LPWStr)]string @string);
        [PreserveSig]int GetSdkPath([Out, MarshalAs(UnmanagedType.BStr)]out string pSdkPath);
        [PreserveSig]int GetTargetLibraryType([Out]out __MIDL___MIDL_itf_CorDebugInterop_1054_0004 pTargetLibraryType);
    }

    [Serializable]
    public enum __MIDL___MIDL_itf_CorDebugInterop_1054_0004
    {
        TLB_Desktop = 1,
        TLB_Starlite = 2,
    }

    [Serializable]
    public enum LoggingLevelEnum
    {
        LTraceLevel0 = 0x0,
        LTraceLevel1 = 0x1,
        LTraceLevel2 = 0x2,
        LTraceLevel3 = 0x3,
        LTraceLevel4 = 0x4,
        LStatusLevel0 = 0x14,
        LStatusLevel1 = 0x15,
        LStatusLevel2 = 0x16,
        LStatusLevel3 = 0x17,
        LStatusLevel4 = 0x18,
        LWarningLevel = 0x28,
        LErrorLevel = 0x32,
        LPanicLevel = 0x64,
    }

    [Serializable]
    public enum AD_PROCESS_ID_TYPE
    {
        AD_PROCESS_ID_SYSTEM = 0,
        AD_PROCESS_ID_GUID = 1,
    }

    [Serializable]
    public enum PROCESS_INFO_FIELDS
    {
        PIF_FILE_NAME = 0x1,
        PIF_BASE_NAME = 0x2,
        PIF_TITLE = 0x4,
        PIF_PROCESS_ID = 0x8,
        PIF_SESSION_ID = 0x10,
        PIF_ATTACHED_SESSION_NAME = 0x20,
        PIF_CREATION_TIME = 0x40,
        PIF_FLAGS = 0x80,
        PIF_ALL = 0xff,
    }

    [Serializable]
    public enum CorDebugExceptionFlags
    {
        DEBUG_EXCEPTION_CAN_BE_INTERCEPTED = 1,
    }

    [Serializable]
    public enum PROCESS_INFO_FLAGS
    {
        PIFLAG_SYSTEM_PROCESS = 0x1,
        PIFLAG_DEBUGGER_ATTACHED = 0x2,
        PIFLAG_PROCESS_STOPPED = 0x4,
        PIFLAG_PROCESS_RUNNING = 0x8,
    }

#pragma warning restore 0108
}
