////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class MemoryStreamHelper
    {
        public static bool WriteReadEmpty(Stream ms)
        {
            bool result = true;
            if (ms.Position != 0)
            {
                result = false;
                Log.Exception("Expected postion 0, but got " + ms.Position);
            }

            if (ms.Length != 0)
            {
                result = false;
                Log.Exception("Expected length 0, but got " + ms.Length);
            }
            return WriteReadVerify(ms) & result;
        }

        public static bool WriteReadVerify(Stream ms)
        {
            bool result = Write(ms, 300);

            // Flush writes
            ms.Flush();

            Log.Comment("Seek to start and Read");
            ms.Seek(0, SeekOrigin.Begin);
            result &= VerifyRead(ms);

            return result;
        }

        public static bool Write(Stream ms, int length)
        {
            bool result = true;
            long startLength = ms.Length;

            // we can only write 0-255, so mod the 
            // length to figure out next data value
            long data = startLength % 256;


            Log.Comment("Seek to end");
            ms.Seek(0, SeekOrigin.End);

            Log.Comment("Write to file");
            for (long i = startLength; i < startLength + length; i++)
            {
                ms.WriteByte((byte)data++);
                // if we hit max byte, reset
                if (data > 255)
                    data = 0;
            }
            return result;
        }

        public static bool VerifyRead(Stream ms)
        {
            bool result = true;
            Log.Comment("Verify " + ms.Length + " bytes of data in file");

            // we can only read 0-255, so mod the 
            // position to figure out next data value
            int nextbyte = (int)ms.Position % 256;

            for (int i = 0; i < ms.Length; i++)
            {
                int readByte = ms.ReadByte();
                if (readByte != nextbyte)
                {
                    result = false;
                    Log.Exception("Byte in position " + i + " has wrong value: " + readByte);
                }

                // Reset if wraps past 255
                if (++nextbyte > 255)
                    nextbyte = 0;
            }
            return result;
        }
    }
}
