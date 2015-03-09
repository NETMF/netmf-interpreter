////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.IO;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;

namespace Microsoft.SPOT.Platform.Tests
{
    public class FileStreamHelper
    {
        public static bool VerifyFile(string file)
        {
            return VerifyFile(file, -1);
        }
        public static bool VerifyFile(string file, long length)
        {
            bool result = true;
            Log.Comment("Open file and verify appended data");
            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // -1 means ignore length check
                if (length == -1)
                    length = fs.Length;

                if (fs.Length != length)
                {
                    result = false;
                    Log.Exception("Expected length of " + length + " but got " + fs.Length);
                }
                result &= VerifyRead(fs);
            }
            return result;
        }

        public static bool WriteReadEmpty(FileStream fs)
        {
            bool result = true;
            if (fs.Position != 0)
            {
                result = false;
                Log.Exception("Expected postion 0, but got " + fs.Position);
            }

            if (fs.Length != 0)
            {
                result = false;
                Log.Exception("Expected length 0, but got " + fs.Position);
            }
            return WriteReadVerify(fs) & result;
        }

        public static bool WriteReadVerify(FileStream fs)
        {
            bool result = Write(fs, 300);

            // Flush writes
            fs.Flush();

            Log.Comment("Seek to start and Read");
            fs.Seek(0, SeekOrigin.Begin);
            result &= VerifyRead(fs);

            return result;
        }

        public static bool Write(FileStream fs, int length)
        {
            bool result = true;
            long startLength = fs.Length;

            // we can only write 0-255, so mod the 
            // length to figure out next data value
            long data = startLength % 256;


            Log.Comment("Seek to end");
            fs.Seek(0, SeekOrigin.End);

            Log.Comment("Write to file");
            for (long i = startLength; i < startLength + length; i++)
            {
                fs.WriteByte((byte)data++);
                // if we hit max byte, reset
                if (data > 255)
                    data = 0;
            }
            return result;
        }

        public static bool VerifyRead(FileStream fs)
        {
            bool result = true;
            Log.Comment("Verify " + fs.Length + " bytes of data in file");

            // we can only read 0-255, so mod the 
            // position to figure out next data value
            int nextbyte = (int)fs.Position % 256;

            for (int i = 0; i < fs.Length; i++)
            {
                int readByte = fs.ReadByte();
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
