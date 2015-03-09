using System;

namespace Microsoft.SPOT.Debugger
{
    internal class DebuggerInterop
    {
        public class Constants
        {
            //            #define guidVSDebugPackage        { 0xC9DD4A57, 0x47FB, 0x11D2, { 0x83, 0xE7, 0x00, 0xC0, 0x4F, 0x99, 0x02, 0xC1 } }
            //#define guidVSDebugGroup          { 0xC9DD4A58, 0x47FB, 0x11D2, { 0x83, 0xE7, 0x00, 0xC0, 0x4F, 0x99, 0x02, 0xC1 } }
            public static Guid guidVSDebugCommand = new Guid( "{0xC9DD4A59,0x47FB,0x11D2,{0x83,0xE7,0x00,0xC0,0x4F,0x99,0x02,0xC1}}" );

            /*
            #define guidDbgOptGeneralPage     { 0xfa9eb535, 0xc624, 0x13d0, { 0xae, 0x1f, 0x00, 0xa0, 0x19, 0x0f, 0xf4, 0xc3 } }
            #define guidDbgOptFindSourcePage  { 0x7a8a4060, 0xd909, 0x4485, { 0x98, 0x60, 0x74, 0x8b, 0xc8, 0x71, 0x3a, 0x74 } }
            #define guidDbgOptFindSymbolPage  { 0xc15095aa, 0x49c0, 0x40ac, { 0xae, 0x78, 0x61, 0x13, 0x18, 0xdd, 0x99, 0x25 } }
            #define guidDbgOptENCPage         { 0x6c3ecaa6, 0x3efb, 0x4b0d, { 0x96, 0x60, 0x2a, 0x3b, 0xa5, 0xb8, 0x44, 0x0e } }
            #define guidDbgOptNativePage      { 0x3e08d829, 0x6989, 0x45c3, { 0x97, 0xda, 0xf8, 0xbf, 0x3c, 0x49, 0xae, 0xab } }
            #define guidDbgOptJITPage         { 0xb9efcaf2, 0x9eae, 0x4022, { 0x9e, 0x39, 0xfa, 0x94, 0x76, 0x66, 0xad, 0xd9 } }

            #define guidDebugOutputPane       { 0xfc076020, 0x078a, 0x11d1, { 0xa7, 0xdf, 0x00, 0xa0, 0xc9, 0x11, 0x00, 0x51 } }
            #define guidDisasmLangSvc         { 0xc16fb7c4, 0x9f84, 0x11d2, { 0x84, 0x05, 0x00, 0xc0, 0x4f, 0x99, 0x02, 0xc1 } }
            #define guidMemoryLangSvc         { 0xdf38847e, 0xcc19, 0x11d2, { 0x8a, 0xda, 0x00, 0xc0, 0x4f, 0x79, 0xe4, 0x79 } }

            #define guidFontColorDisassembly  { 0x9e632e6e, 0xd786, 0x4f9a, { 0x8d, 0x3e, 0xb9, 0x39, 0x88, 0x36, 0xc7, 0x84 } }
            #define guidFontColorMemory       { 0xce2eced5, 0xc21c, 0x464c, { 0x9b, 0x45, 0x15, 0xe1, 0x0e, 0x9f, 0x9e, 0xf9 } }
            #define guidFontColorRegisters    { 0x40660f54, 0x80fa, 0x4375, { 0x89, 0xa3, 0x8d, 0x06, 0xaa, 0x95, 0x4e, 0xba } }

            #define guidDebuggerFontColorSvc { 0x3b70a4ae, 0xbb91, 0x4abe, { 0xa0, 0x5c, 0xc4, 0xde, 0x7, 0xb9, 0x76, 0x3e } }
            #define guidWatchFontColor		{ 0x358463d0, 0xd084, 0x400f, { 0x99, 0x7e, 0xa3, 0x4f, 0xc5, 0x70, 0xbc, 0x72 } }
            #define guidAutosFontColor		{ 0xa7ee6bee, 0xd0aa, 0x4b2f, { 0xad, 0x9d, 0x74, 0x82, 0x76, 0xa7, 0x25, 0xf6 } }
            #define guidLocalsFontColor		{ 0x8259aced, 0x490a, 0x41b3, { 0xa0, 0xfb, 0x64, 0xc8, 0x42, 0xcc, 0xdc, 0x80 } }
            #define guidCallStackFontColor	{ 0xfd2219af, 0xebf8, 0x4116, { 0xa8, 0x1, 0x3b, 0x50, 0x3c, 0x48, 0xdf, 0xf0  } }
            #define guidThreadsFontColor	{ 0xbb8fe807, 0xa186, 0x404a, { 0x81, 0xfa, 0xd2, 0xb, 0x90, 0x8c, 0xa9, 0x3b  } }
            #define guidDataTipsFontColor	{ 0xf7b7b222, 0xe186, 0x48df, { 0xa5, 0xee, 0x17, 0x4e, 0x81, 0x29, 0x89, 0x1b } }
            #define guidVarWndsFontColor	{ 0x35b25e75, 0xab53, 0x4c5d, { 0x80, 0xea, 0x66, 0x82, 0xeb, 0xb2, 0xbb, 0xbd } };
            */

            public enum VSDebugCmdID : uint
            {
                //DebuggerFirst = 0x00000000,
                //DebuggerLast = 0x00000fff,

                //BreakpointsWindowShow = 0x00000100,
                DisasmWindowShow = 0x00000101,
                //ProgramToDebugShow = 0x00000102,
                RegisterWindowShow = 0x00000103,
                //ModulesWindowShow = 0x00000104,
                //ApplyCodeChanges = 0x00000105,
                //StopApplyCodeChanges = 0x00000106,
                GoToDisassembly = 0x00000107,
                //ShowDebugOutput = 0x00000108,
                //StepUnitLine = 0x00000110,
                //StepUnitStatement = 0x00000111,
                //StepUnitInstruction = 0x00000112,
                //StepUnitList = 0x00000113,
                //StepUnitListEnum = 0x00000114,
                //WriteCrashDump = 0x00000115,
                //ProcessList = 0x00000116,
                //ProcessListEnum = 0x00000117,
                //ThreadList = 0x00000118,
                //ThreadListEnum = 0x00000119,
                //StackFrameList = 0x00000120,
                //StackFrameListEnum = 0x00000121,
                //DisableAllBreakpoints = 0x00000122,
                //EnableAllBreakpoints = 0x00000123,
                //ToggleAllBreakpoints = 0x00000124,
                //TerminateAll = 0x00000125,
                //SymbolOptions = 0x00000126,
                //LoadSymbols = 0x00000127,
                //SymbolLoadInfo = 0x00000128,
                //StopEvaluatingExpression = 0x00000129,
                //ConsoleWindowShow = 0x00000130,
                //AttachedProcsWindowShow = 0x00000131,
                //// unused                                   = 0x00000132,
                //// unused                                   = 0x00000133,
                //// unused                                   = 0x00000134,
                //JustMyCode = 0x00000135,
                //NewFileBreakpoint = 0x00000136,
                //NewFunctionBreakpoint = 0x00000137,
                //NewAddressBreakpoint = 0x00000138,
                //NewDataBreakpoint = 0x00000139,
                //// unused                                   = 0x00000140,
                //InsertTracepoint = 0x00000041,
                //BreakpointLocation = 0x00000142,
                //BreakpointCondition = 0x00000143,
                //BreakpointHitCount = 0x00000144,
                //BreakpointConstraints = 0x00000145,
                //BreakpointAction = 0x00000146,
                //CreateObjectID = 0x00000147,
                //RunMacrosForBreakpointsJustHit = 0x00000148,
                //CopyExpression = 0x00000149,
                //CopyValue = 0x0000014A,
                //DestroyObjectID = 0x0000014B,
                //OutputOnException = 0x00000150,
                //OutputOnModuleLoad = 0x00000151,
                //OutputOnModuleUnload = 0x00000152,
                //OutputOnProcessDestroy = 0x00000153,
                //OutputOnThreadDestroy = 0x00000154,
                //OutputOnOutputDebugString = 0x00000155,
                //SingleProcStepInto = 0x00000156,
                //SingleProcStepOver = 0x00000157,
                //SingleProcStepOut = 0x00000158,


                //// See above for explanation of these constants...
                //MemoryWindowShow = 0x00000200,
                //MemoryWindowShow1 = 0x01000200,
                //MemoryWindowShow2 = 0x02000200,
                //MemoryWindowShow3 = 0x03000200,
                //MemoryWindowShow4 = 0x04000200,

                //WatchWindowShow = 0x00000300,
                //WatchWindowShow1 = 0x01000300,
                //WatchWindowShow2 = 0x02000300,
                //WatchWindowShow3 = 0x03000300,
                //WatchWindowShow4 = 0x04000300,

                //// Breakpoint Window commands
                //BreakpointsWindowFirst = 0x00001000,
                //BreakpointsWindowLast = 0x00001fff,

                //BreakpointsWindowNewBreakpoint = 0x00001001, // deprecated
                //BreakpointsWindowNewGroup = 0x00001002,
                //BreakpointsWindowDelete = 0x00001003,
                //BreakpointsWindowProperties = 0x00001004, // deprecated
                //BreakpointsWindowDefaultGroup = 0x00001005,
                //BreakpointsWindowGoToSource = 0x00001006,
                BreakpointsWindowGoToDisassembly = 0x00001007,
                //BreakpointsWindowGoToBreakpoint = 0x00001008,

                //BreakpointsWindowColumnName = 0x00001100,
                //BreakpointsWindowColumnCondition = 0x00001101,
                //BreakpointsWindowColumnHitCount = 0x00001102,
                //BreakpointsWindowColumnLanguage = 0x00001103,
                //BreakpointsWindowColumnFunction = 0x00001104,
                //BreakpointsWindowColumnFile = 0x00001105,
                //BreakpointsWindowColumnAddress = 0x00001106,
                //BreakpointsWindowColumnData = 0x00001107,
                //BreakpointsWindowColumnProcess = 0x00001108,
                //BreakpointsWindowColumnConstraints = 0x00001109,
                //BreakpointsWindowColumnAction = 0x0000110A,


                //// Disassembly Window commands
                //DisasmWindowFirst = 0x00002000,
                //DisasmWindowLast = 0x00002fff,

                //GoToSource = 0x00002001,
                //ShowDisasmAddress = 0x00002002,
                //ShowDisasmSource = 0x00002003,
                //ShowDisasmCodeBytes = 0x00002004,
                //ShowDisasmSymbolNames = 0x00002005,
                //ShowDisasmLineNumbers = 0x00002006,
                //ShowDisasmToolbar = 0x00002007,
                //DisasmExpression = 0x00002008,
                ToggleDisassembly = 0x00002009,

                //// Memory Window commands
                //MemoryWindowFirst = 0x00003000,
                //MemoryWindowLast = 0x00003fff,

                //// The following are specific to each instance of the memory window.  The high-end
                //// byte is critical for proper operation of these commands.  The high-byte indicates
                //// the particular toolwindow that this cmdid applies to.  You can change the
                //// lowest 3 bytes to be whatever you want.

                //// The first constant in each group marks a cmdid representing the entire group.
                //// We use this constant inside our switch statements so as to not have to list
                //// out each separate instance of cmdid.
                //MemoryExpression = 0x00003001,
                //MemoryExpression1 = 0x01003001,
                //MemoryExpression2 = 0x02003001,
                //MemoryExpression3 = 0x03003001,
                //MemoryExpression4 = 0x04003001,

                //AutoReevaluate = 0x00003002,
                //AutoReevaluate1 = 0x01003002,
                //AutoReevaluate2 = 0x02003002,
                //AutoReevaluate3 = 0x03003002,
                //AutoReevaluate4 = 0x04003002,

                //MemoryColumns = 0x00003003,
                //MemoryColumns1 = 0x01003003,
                //MemoryColumns2 = 0x02003003,
                //MemoryColumns3 = 0x03003003,
                //MemoryColumns4 = 0x04003003,

                //ColCountList = 0x00003004,
                //ColCountList1 = 0x01003004,
                //ColCountList2 = 0x02003004,
                //ColCountList3 = 0x03003004,
                //ColCountList4 = 0x04003004,

                //// The following apply to all instances of the memory windows.  If any of these
                //// are added to the toolbar, they must be made per-instance!
                //ShowNoData = 0x00003011,
                //OneByteInt = 0x00003012,
                //TwoByteInt = 0x00003013,
                //FourByteInt = 0x00003014,
                //EightByteInt = 0x00003015,
                //Float = 0x00003020,
                //Double = 0x00003021,
                //FormatHex = 0x00003030,
                //FormatSigned = 0x00003031,
                //FormatUnsigned = 0x00003032,
                //FormatBigEndian = 0x00003033,
                //ShowNoText = 0x00003040,
                //ShowAnsiText = 0x00003041,
                //ShowUnicodeText = 0x00003042,
                //EditValue = 0x00003050,
                //ShowToolbar = 0x00003062,

                //// MemoryView-specific commands.  These are used internally by the MemoryView implementation.
                //StopInPlaceEdit = 0x00003100,

                //// Registers Window commands
                //RegisterWindowFirst = 0x00004000,
                RegWinGroupFirst = 0x00004001,
                //RegWinGroupLast = 0x00004100,

                //RegisterWindowLast = 0x00004fff,

                //// QuickWatch commands
                //QuickWatchFirst = 0x00005000,
                //QuickWatchLast = 0x00005fff,

                //// Debug Context toolbar commands
                ////DebugContextFirst              = 0x00006000,
                ////DebugContextLast               = 0x00006fff,


                //// Modules Window commands
                //ModulesWindowFirst = 0x00007000,
                //ModulesWindowLast = 0x00007100,

                //ReloadSymbols = 0x00007001, // deprecated
                //ShowAllModules = 0x00007002,
                //ToggleUserCode = 0x00007003,

                //// step into specific
                //StepIntoSpecificFirst = 0x00007200,
                //StepIntoSpecificLast = 0x00007FFF,

                //// Call Stack commands
                //CallStackWindowFirst = 0x00008000,
                //CallStackWindowLast = 0x00008fff,

                //SetCurrentFrame = 0x00008001,
                //CallStackValues = 0x00008002,
                //CallStackTypes = 0x00008003,
                //CallStackNames = 0x00008004,
                //CallStackModules = 0x00008005,
                //CallStackLineOffset = 0x00008006,
                //CallStackByteOffset = 0x00008007,
                //CrossThreadCallStack = 0x00008008,
                //ShowExternalCode = 0x00008009,
                //UnwindFromException = 0x0000800a,

                //// Datatip commands
                //DatatipFirst = 0x00009000,
                //DatatipLast = 0x00009fff,

                //DatatipNoTransparency = 0x00009010,
                //DatatipLowTransparency = 0x00009011,
                //DatatipMedTransparency = 0x00009012,
                //DatatipHighTransparency = 0x00009013,

                //// Attached Processes Window commands
                //AttachedProcsWindowFirst = 0x0000a000,
                //AttachedProcsWindowLast = 0x0000a100,

                //AttachedProcsStartProcess = 0x0000a001,
                //AttachedProcsPauseProcess = 0x0000a002,
                //AttachedProcsStepIntoProcess = 0x0000a003,
                //AttachedProcsStepOverProcess = 0x0000a004,
                //AttachedProcsStepOutProcess = 0x0000a005,
                //AttachedProcsDetachProcess = 0x0000a006,
                //AttachedProcsTerminateProcess = 0x0000a007,
                //AttachedProcsDetachOnStop = 0x0000a008,
                //AttachedProcsColumnName = 0x0000a010,
                //AttachedProcsColumnID = 0x0000a011,
                //AttachedProcsColumnPath = 0x0000a012,
                //AttachedProcsColumnTitle = 0x0000a013,
                //AttachedProcsColumnMachine = 0x0000a014,
                //AttachedProcsColumnState = 0x0000a015,
                //AttachedProcsColumnTransport = 0x0000a016,
                //AttachedProcsColumnTransportQualifier = 0x0000a017,

                //// Console Window commands
                //ConsoleWindowFirst = 0x0000b000,
                //ConsoleWindowLast = 0x0000bfff,

                //ConsoleSendEndOfFile = 0x0000b001,


                //// Command Window commands
                //// while all commands are available in the command window,
                //// these are not on any menus by default
                ////
                //CommandWindowFirst = 0x0000f000,
                //CommandWindowLast = 0x0000ffff,

                //ListMemory = 0x0000f001,
                //ListCallStack = 0x0000f002,
                ListDisassembly = 0x0000f003,
                ListRegisters = 0x0000f004,
                //// unused                                   = 0x0000f005,
                //ListThreads = 0x0000f006,
                //SetRadix = 0x0000f007,
                //// unused                                   = 0x0000f008,
                //SetCurrentThread = 0x0000f009,
                //SetCurrentStackFrame = 0x0000f00a,
                //ListSource = 0x0000f00b,
                //SymbolPath = 0x0000f00c,
                //ListModules = 0x0000f00d,
                //ListProcesses = 0x0000f00e,
                //SetCurrentProcess = 0x0000f00f,

            }
        }
    }
}