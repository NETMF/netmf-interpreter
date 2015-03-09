////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using Microsoft.SPOT.Emulator.Com;
using Microsoft.SPOT.Hardware;
using System.Collections;


namespace Microsoft.SPOT.Emulator.Serial
{
    internal class SerialDriver : HalDriver<ISerialDriver>, ISerialDriver
    {
        OnSerialPortEvtHandler _evtHandler;

        private ComPort GetComPort( int serialPortNum )
        {
            // serialPortNum is 0-based (i.e. COM1 is 0, COM2 is 1) and ComPortHandle is 1-based (COM1 is (Usart, 1), COM2 is (Usart, 2))
            return this.Emulator.ComPortCollection.DeviceGet(new ComPortHandle(TransportType.Usart, serialPortNum + 1));
        }

        #region ISerialDriver

        bool ISerialDriver.Initialize(int serialPortNum, int BaudRate, int Parity, int DataBits, int StopBits, int FlowValue)
        {
            ComPort serial = null;
            
            try
            {
                serial = GetComPort( serialPortNum );
            }
            catch
            {
                return false;
            }
            

            if(!(serial is ISerialPortToStream))
            {
                return false;
            }
            
            return ((ISerialPortToStream)serial).Initialize(BaudRate, Parity, DataBits, StopBits, FlowValue) && serial.DeviceInitialize();
        }

        bool ISerialDriver.Uninitialize(int serialPortNum)
        {
            ComPort serial = GetComPort( serialPortNum );

            if(!(serial is ISerialPortToStream)) return false;

            return ((ISerialPortToStream)serial).Uninitialize() && serial.DeviceUninitialize();
        }

        int ISerialDriver.Write(int serialPortNum, IntPtr Data, uint size)
        {
            byte[] buffer = Utility.MarshalBytes( Data, (int)size );

            return GetComPort( serialPortNum ).DeviceWrite( buffer );
        }

        int ISerialDriver.Read(int serialPortNum, IntPtr Data, uint size)
        {
            byte[] buffer = new byte[size];

            int bytesRead = GetComPort( serialPortNum ).DeviceRead( buffer );

            if (bytesRead > 0)
            {
                Marshal.Copy(buffer, 0, Data, bytesRead);
            }

            return bytesRead;
        }

        bool ISerialDriver.Flush(int serialPortNum)
        {
            return GetComPort( serialPortNum ).DeviceFlush();
        }

        bool ISerialDriver.AddCharToRxBuffer(int ComPortNum, char c)
        {
            return false;
        }

        bool ISerialDriver.RemoveCharFromTxBuffer(int ComPortNum, ref char c)
        {
            return false;
        }

        byte ISerialDriver.PowerSave(int ComPortNum, byte Enable)
        {
            return 0;
        }

        void ISerialDriver.PrepareForClockStop()
        {
        }

        void ISerialDriver.ClockStopFinished()
        {
        }

        void ISerialDriver.CloseAllPorts()
        {
            // should close serial ports firstly for soft reboot
        }

        int ISerialDriver.BytesInBuffer( int serialPortNum, bool fRx )
        {
            if (fRx)
            {
                ComPortToStream port = GetComPort(serialPortNum) as ComPortToStream;

                if (port != null)
                {
                    return port._fifoToDevice.Available;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        
        void ISerialDriver.DiscardBuffer( int serialPortNum, bool fRx )
        {
            if (fRx)
            {
                byte[] buf = new byte[1024];

                ComPortToStream port = GetComPort(serialPortNum) as ComPortToStream;

                if (port != null)
                {
                    while (port.AvailableBytes > 0)
                    {
                        port.DeviceRead(buf);
                    }

                    while (port._fifoToDevice.Available > 0)
                    {
                        port._fifoToDevice.Read(buf, 0, buf.Length);
                    }
                }
            }
        }
        
        uint ISerialDriver.PortsCount()
        {
            return (uint)this.Emulator.SerialPorts.Count;
        }

        void ISerialDriver.GetPins(int serialPortNum, out uint rxPin, out uint txPin, out uint ctsPin, out uint rtsPin)
        {
            ComPort comPort = GetComPort(serialPortNum);
            rxPin = (uint)comPort.RxPin;
            txPin = (uint)comPort.TxPin;
            ctsPin = (uint)comPort.CtsPin;
            rtsPin = (uint)comPort.RtsPin;
        }

        bool ISerialDriver.SupportNonStandardBaudRate(int serialPortNum)
        {
            return GetComPort(serialPortNum).SupportNonStandardBaudRate;
        }

        void ISerialDriver.BaudrateBoundary(int serialPortNum, out uint maxBaudrateHz, out uint minBaudrateHz)
        {
            ComPort comPort = GetComPort(serialPortNum);
            maxBaudrateHz = comPort.MaxBaudrateHz;
            minBaudrateHz = comPort.MinBaudrateHz;
        }

        bool ISerialDriver.IsBaudrateSupported(int serialPortNum, ref uint BaudrateHz)
        {
            ComPort comPort = GetComPort(serialPortNum);
            if ((BaudrateHz >= comPort.MinBaudrateHz) && (BaudrateHz <= comPort.MaxBaudrateHz))
                return true;

            BaudrateHz = comPort.MaxBaudrateHz;
            return false;
        }

        bool ISerialDriver.SetDataEventHandler(int serialPortNum, IntPtr handler )
        {
            _evtHandler = (OnSerialPortEvtHandler)Marshal.GetDelegateForFunctionPointer(handler, typeof(OnSerialPortEvtHandler));

            ISerialPortToStream serialPort = GetComPort(serialPortNum) as ISerialPortToStream;

            if(serialPort != null)
            {
                serialPort.SetDataEventHandler(new OnEmuSerialPortEvtHandler(_evtHandler));
            }

            return true;
        }


        #endregion
    }

    // SerialPortCollection is a collection which purpose is to assist the user to easily locate serial ports.
    // Note that it is _NOT_ an EmulatorComponentCollection, as it doesn't not actually contains any components, but just redirect
    // them to the ComPortCollection.
    public class SerialPortCollection : EmulatorComponent, ICollection
    {
        ComPortCollection _comPortCollection;

        public override void SetupComponent()
        {
            _comPortCollection = this.Emulator.ComPortCollection;
        }
        
        public ComPort this[int portNum]
        {
            get
            {
                return _comPortCollection[new ComPortHandle( TransportType.Usart, portNum )];
            }
        }

        public override bool IsReplaceableBy( EmulatorComponent ec )
        {
            return (ec is SerialPortCollection);
        }

        internal int Count
        {
            get
            {
                return _comPortCollection.GetAll(TransportType.Usart).Count;
            }
        }

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ComPort[] comPortArray = array as ComPort[];
            if (comPortArray == null)
            {
                throw new ArgumentException("Cannot cast array into ComPort[]");
            }
            _comPortCollection.GetAll(TransportType.Usart).CopyTo(comPortArray, index);
        }

        int ICollection.Count
        {
            get 
            {
                return _comPortCollection.GetAll(TransportType.Usart).Count; 
            }
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return this; }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _comPortCollection.GetAll(TransportType.Usart).GetEnumerator();
        }

        #endregion
    }
}
