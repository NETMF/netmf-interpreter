////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

using Microsoft.SPOT.Emulator.Com;
using Microsoft.SPOT.Emulator.Gpio;
using Microsoft.SPOT.Emulator.Spi;
using Microsoft.SPOT.Emulator.Serial;
using Microsoft.SPOT.Emulator.Lcd;
using Microsoft.SPOT.Emulator.Time;
using Microsoft.SPOT.Emulator.Events;
using Microsoft.SPOT.Emulator.Usb;
using Microsoft.SPOT.Emulator.Memory;
using Microsoft.SPOT.Emulator.Sockets;
using Microsoft.SPOT.Emulator.I2c;
using Microsoft.SPOT.Emulator.Battery;

namespace Microsoft.SPOT.Emulator
{
    // Note that this enum has to be kept in sync with the HRESULTs
    // defined in netmf_errorcodes.h.
    public enum CLR_ERRORCODE : uint
    {
        CLR_E_UNKNOWN_INSTRUCTION                   = 0x80000000 | 0x01000000 | 0x0000,
        CLR_E_UNSUPPORTED_INSTRUCTION               = 0x80000000 | 0x02000000 | 0x0000,

        CLR_E_STACK_OVERFLOW                        = 0x80000000 | 0x11000000 | 0x0000,
        CLR_E_STACK_UNDERFLOW                       = 0x80000000 | 0x12000000 | 0x0000,
        
        CLR_E_ENTRY_NOT_FOUND                       = 0x80000000 | 0x15000000 | 0x0000,
        CLR_E_ASSM_WRONG_CHECKSUM                   = 0x80000000 | 0x16000000 | 0x0000,
        CLR_E_ASSM_PATCHING_NOT_SUPPORTED           = 0x80000000 | 0x17000000 | 0x0000,
        CLR_E_SHUTTING_DOWN                         = 0x80000000 | 0x18000000 | 0x0000,
        CLR_E_OBJECT_DISPOSED                       = 0x80000000 | 0x19000000 | 0x0000,
        CLR_E_WATCHDOG_TIMEOUT                      = 0x80000000 | 0x1A000000 | 0x0000,

        CLR_E_NULL_REFERENCE                        = 0x80000000 | 0x21000000 | 0x0000,
        CLR_E_WRONG_TYPE                            = 0x80000000 | 0x22000000 | 0x0000,
        CLR_E_TYPE_UNAVAILABLE                      = 0x80000000 | 0x23000000 | 0x0000,
        CLR_E_INVALID_CAST                          = 0x80000000 | 0x24000000 | 0x0000,
        CLR_E_OUT_OF_RANGE                          = 0x80000000 | 0x25000000 | 0x0000,
        
        CLR_E_SERIALIZATION_VIOLATION               = 0x80000000 | 0x27000000 | 0x0000,
        CLR_E_SERIALIZATION_BADSTREAM               = 0x80000000 | 0x28000000 | 0x0000,
        CLR_E_INDEX_OUT_OF_RANGE                    = 0x80000000 | 0x29000000 | 0x0000,

        CLR_E_DIVIDE_BY_ZERO                        = 0x80000000 | 0x31000000 | 0x0000,

        CLR_E_BUSY                                  = 0x80000000 | 0x33000000 | 0x0000,

        CLR_E_PROCESS_EXCEPTION                     = 0x80000000 | 0x41000000 | 0x0000,

        CLR_E_THREAD_WAITING                        = 0x80000000 | 0x42000000 | 0x0000,

        CLR_E_LOCK_SYNCHRONIZATION_EXCEPTION        = 0x80000000 | 0x44000000 | 0x0000,

        CLR_E_APPDOMAIN_EXITED                      = 0x80000000 | 0x48000000 | 0x0000,
        CLR_E_APPDOMAIN_MARSHAL_EXCEPTION           = 0x80000000 | 0x49000000 | 0x0000,
        CLR_E_NOTIMPL                               = 0x80000000 | 0x4a000000 | 0x0000,
        
        CLR_E_UNKNOWN_TYPE                          = 0x80000000 | 0x4d000000 | 0x0000,
        CLR_E_ARGUMENT_NULL                         = 0x80000000 | 0x4e000000 | 0x0000,
        CLR_E_IO                                    = 0x80000000 | 0x4f000000 | 0x0000,

        CLR_E_ENTRYPOINT_NOT_FOUND                  = 0x80000000 | 0x50000000 | 0x0000,
        CLR_E_DRIVER_NOT_REGISTERED                 = 0x80000000 | 0x51000000 | 0x0000,


//
// Gp IO error codes
//
        CLR_E_PIN_UNAVAILABLE                       = 0x80000000 | 0x54000000 | 0x0000,
        CLR_E_PIN_DEAD                              = 0x80000000 | 0x55000000 | 0x0000,
        CLR_E_INVALID_OPERATION                     = 0x80000000 | 0x56000000 | 0x0000,
        CLR_E_WRONG_INTERRUPT_TYPE                  = 0x80000000 | 0x57000000 | 0x0000,
        CLR_E_NO_INTERRUPT                          = 0x80000000 | 0x58000000 | 0x0000,
        CLR_E_DISPATCHER_ACTIVE                     = 0x80000000 | 0x59000000 | 0x0000,


//
// IO error codes 
//

        CLR_E_FILE_IO                               = 0x80000000 | 0x60000000 | 0x0000,
        CLR_E_INVALID_DRIVER                        = 0x80000000 | 0x61000000 | 0x0000,
        CLR_E_FILE_NOT_FOUND                        = 0x80000000 | 0x62000000 | 0x0000,
        CLR_E_DIRECTORY_NOT_FOUND                   = 0x80000000 | 0x63000000 | 0x0000,
        CLR_E_VOLUME_NOT_FOUND                      = 0x80000000 | 0x64000000 | 0x0000,
        CLR_E_PATH_TOO_LONG                         = 0x80000000 | 0x65000000 | 0x0000,
        CLR_E_DIRECTORY_NOT_EMPTY                   = 0x80000000 | 0x66000000 | 0x0000,
        CLR_E_UNAUTHORIZED_ACCESS                   = 0x80000000 | 0x67000000 | 0x0000,
        CLR_E_PATH_ALREADY_EXISTS                   = 0x80000000 | 0x68000000 | 0x0000,
        CLR_E_TOO_MANY_OPEN_HANDLES                 = 0x80000000 | 0x69000000 | 0x0000,

//
// General error codes
//

        CLR_E_NOT_SUPPORTED                         = 0x80000000 | 0x77000000 | 0x0000,
        CLR_E_RESCHEDULE                            = 0x80000000 | 0x78000000 | 0x0000,
        
        CLR_E_OUT_OF_MEMORY                         = 0x80000000 | 0x7A000000 | 0x0000,
        CLR_E_RESTART_EXECUTION                     = 0x80000000 | 0x7B000000 | 0x0000,

        CLR_E_INVALID_PARAMETER                     = 0x80000000 | 0x7D000000 | 0x0000,
        CLR_E_TIMEOUT                               = 0x80000000 | 0x7E000000 | 0x0000,
        CLR_E_FAIL                                  = 0x80000000 | 0x7f000000 | 0x0000,

//--//

        CLR_S_THREAD_EXITED                         = 0x01000000 | 0x0000,
        CLR_S_QUANTUM_EXPIRED                       = 0x02000000 | 0x0000,
        CLR_S_NO_READY_THREADS                      = 0x03000000 | 0x0000,
        CLR_S_NO_THREADS                            = 0x04000000 | 0x0000,
        CLR_S_RESTART_EXECUTION                     = 0x05000000 | 0x0000,

//
// PARSER error codes.
//
        CLR_E_PARSER_BAD_TEXT_SIGNATURE             = 0x80000000 | 0x01010000 | 0x0001,
        CLR_E_PARSER_BAD_CUSTOM_ATTRIBUTE           = 0x80000000 | 0x01010000 | 0x0002,
        CLR_E_PARSER_UNSUPPORTED_MULTIDIM_ARRAY     = 0x80000000 | 0x01010000 | 0x0003,
        CLR_E_PARSER_UNKNOWN_MEMBER_REF             = 0x80000000 | 0x01010000 | 0x0004,
        CLR_E_PARSER_MISSING_FIELD                  = 0x80000000 | 0x01010000 | 0x0005,
        CLR_E_PARSER_MISSING_METHOD                 = 0x80000000 | 0x01010000 | 0x0006,
        CLR_E_PARSER_MISSING_INTERFACE              = 0x80000000 | 0x01010000 | 0x0007,
        CLR_E_PARSER_MISSING_TOKEN                  = 0x80000000 | 0x01010000 | 0x0008,
        CLR_E_PARSER_UNSUPPORTED_GENERICS           = 0x80000000 | 0x01010000 | 0x0009
    }

    public class EmulatorException : Exception
    {
        public EmulatorException(int errorCode)
            : base()
        {
            this.HResult = errorCode;
        }

        public EmulatorException(String message, int errorCode)
            : base(message)
        {
            this.HResult = errorCode;
        }

        public CLR_ERRORCODE ErrorCode
        {
            get { return (CLR_ERRORCODE)this.HResult; }
        }

        public override string Message
        {
            get
            {
                CLR_ERRORCODE r = (CLR_ERRORCODE)this.HResult;

                String message = "";

                if (!String.IsNullOrEmpty(base.Message))
                    message = base.Message + ": ";

                String m = "";
                switch(r)
                {
                    case CLR_ERRORCODE.CLR_E_ENTRYPOINT_NOT_FOUND:
                        m = "Entrypoint not found: Please double check the assemblies specified.";
                        break;

                    default:
                        m = r.ToString();
                        break;
                }

                return message + m;
            }
        }
    }
}
