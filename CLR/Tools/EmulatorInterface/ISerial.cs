////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;

namespace Microsoft.SPOT.Emulator.Serial
{
    public delegate void OnSerialPortEvtHandler(int port, uint evt);

    public interface ISerialDriver
    {
        bool Initialize(int ComPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue);
        bool Uninitialize(int ComPortNum);
        int  Write(int ComPortNum, IntPtr Data, uint size);
        int  Read( int ComPortNum, IntPtr Data, uint size );
        bool Flush( int ComPortNum );
        bool AddCharToRxBuffer(int ComPortNum, char c );
        bool RemoveCharFromTxBuffer( int ComPortNum, ref char c );
        byte PowerSave( int ComPortNum, byte Enable );
        void PrepareForClockStop();
        void ClockStopFinished();
        void CloseAllPorts();
        int  BytesInBuffer( int ComPortNum, bool fRx );
        void DiscardBuffer( int ComPortNum, bool fRx );
        uint PortsCount();
        void GetPins(int ComPortNum, out uint rxPin, out uint txPin, out uint ctsPin, out uint rtsPin);
        bool SupportNonStandardBaudRate (int ComPortNum);
        void BaudrateBoundary(int ComPortNum, out uint maxBaudrateHz, out uint minBaudrateHz);
        bool IsBaudrateSupported(int ComPortNum, ref uint BaudrateHz);
        bool SetDataEventHandler(int ComPortNum, IntPtr handler );
        
    }
}
