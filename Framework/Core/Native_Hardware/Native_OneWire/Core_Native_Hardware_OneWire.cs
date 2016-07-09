using System;
using Microsoft.SPOT;
using System.Runtime.CompilerServices;
using System.Collections;

namespace Microsoft.SPOT.Hardware
{
    public sealed class OneWire
    {
        // external One Wire functions from link layer owllu.c
        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchReset();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchBit(int sendbit);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int TouchByte(int sendbyte);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int WriteByte(int sendbyte);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int ReadByte();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int AcquireEx();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int Release();

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindFirstDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int FindNextDevice(bool performResetBeforeSearch, bool searchWithAlarmCommand);

        [MethodImpl(MethodImplOptions.InternalCall)]
        public extern int SerialNum(byte[] SNum, bool read);

        [MethodImplAttribute(MethodImplOptions.Synchronized)]
        public ArrayList FindAllDevices()
        {
            int rslt;
 
            // attempt to acquire the 1-Wire Net
            if ((rslt = AcquireEx()) < 0)
            {
                //OWERROR_DUMP(stdout);

                // could not get access to 1-wire buss, return null
                return null;
            }

            ArrayList serialNumbers = new ArrayList();

            // find the first device (all devices not just alarming)
            rslt = FindFirstDevice(true, false);
            while (rslt != 0)
            {
                byte[] SNum = new byte[8];

                // retrieve the serial number just found
                SerialNum(SNum, true);

                // save serial number
                serialNumbers.Add(SNum);

                // find the next device
                rslt = FindNextDevice(true, false);
            }

            // release the 1-Wire Net
            Release();

            return serialNumbers;
        }

        uint _pin; 						// The native code only needs pin number
        uint _logicalPort; 				// Handle used for subsequent calls. 
		const uint MAX_PORTNUM  = 16;	// Must be the same as defined in "DeviceCode\pal\OneWire\DallasSemi\ownet.h"

        public OneWire(uint logicalPort, OutputPort port)
        {
            _pin = (uint)port.Id; 		// the pin number is enough to identify the port on the native side
			_logicalPort = logicalPort; // 0 .. MAX_PORTNUM    (default .NetMF is 16 logical ports)
        }
    }
}
