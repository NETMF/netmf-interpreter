////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;

namespace Microsoft.SPOT.Platform.Tests
{
    using System;
    using Microsoft.SPOT;
    using System.Threading;
    using Microsoft.SPOT.Hardware;



    public class EEPROM_I2C_Page
    {
        private byte[] PageData;
        private int pageLength;
        private ushort address;
        public EEPROM_I2C_Page()
        {
            PageData = new byte[34];
        }
        public ushort Address
        {
            get { return address; }
            set
            {
                address = value;
                PageData[0] = (byte)((Address & 0xFF00) >> 8);
                PageData[1] = (byte)(Address & 0xFF);
            }
        }

        public int Length
        {
            get { return pageLength; }
        }
        public void StorePage(ushort Address, byte[] Buffer)
        {
            for (int offset = 0; offset < Buffer.Length; offset++)
            {
                PageData[2 + offset] = Buffer[offset];
            }
            pageLength = Buffer.Length;
            this.Address = Address;
        }
        public void LoadPage(ref byte[] Buffer)
        {
            if (Buffer.Length >= pageLength)
            {
                for (int offset = 0; offset < pageLength; offset++)
                {
                    Buffer[offset] = PageData[offset + 2];
                }
            }
        }
        public byte[] Page()
        {
            return PageData;
        }
        Boolean Equals(byte[] data)
        {
            int len = data.Length < this.Length ? data.Length : this.Length;
            for (int offset = 0; offset < len; offset++)
            {
                if (data[offset] != this.PageData[offset + 2])
                {
                    return false;
                }
            }
            return true;
        }
    } //EEPROM_I2C_Page class 

    public class EEPROM_I2C
    {
        private I2CDevice I2C_device;
        const byte EEPROM_I2C_1 = 0x51;
        const byte EEPROM_I2C_2 = 0x54;
        const ushort EEPROM_Size = 32 * 1024;
        private byte I2C_Address;
        private byte I2C_ClockRate;
        private int m_transfer_count = 0;
        private transfer_type m_transfer_type;
        private int m_timeout = 20;

        public enum transfer_type { read, write };

        public EEPROM_I2C()
        {
            I2C_Address = EEPROM_I2C_1;
            I2C_ClockRate = 100;
            I2CDevice.Configuration config = new I2CDevice.Configuration(I2C_Address, I2C_ClockRate);
            I2C_device = new I2CDevice(config);
        }
        public EEPROM_I2C(byte subordinate_address, byte subordinate_clockrate)
        {
            I2C_Address = subordinate_address;
            I2C_ClockRate = subordinate_clockrate;
            I2CDevice.Configuration config = new I2CDevice.Configuration(I2C_Address, I2C_ClockRate);
            I2C_device = new I2CDevice(config);
        }
        public void Dispose()
        {
            I2C_device.Dispose();
        }

        public byte HighAddress(ushort Address)
        {
            return (byte)((Address & 0xFF00) >> 8);
        }
        public byte LowAddress(ushort Address)
        {
            return (byte)(Address & 0xFF);
        }
        public ushort PageAddress(ushort Address)
        {
            return (ushort)(Address & 0xFFE0);
        }
        public transfer_type Transfer
        {
            get { return m_transfer_type; }
        }

        public int Count
        {
            get { return m_transfer_count; }
            //set { m_transfer_count = value; }
        }
        public int Timeout
        {
            set { m_timeout = value; }
        }

        public byte CurrentByteRead()
        {
            byte[] readBuffer = { 0x0 };
            I2CDevice.I2CReadTransaction read = I2CDevice.CreateReadTransaction(readBuffer);
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { read };
            m_transfer_count = I2C_device.Execute(transactions, m_timeout);
            m_transfer_type = transfer_type.read;
            return readBuffer[0];
        }
        public byte RandomByteRead(ushort address)
        {
            byte[] dummyAddress = { HighAddress(address), LowAddress(address) };
            byte[] readBuffer = { 0x0 };
            //
            // non-data write relocates internal EEPROM counter for random read
            //
            I2CDevice.I2CReadTransaction read = I2CDevice.CreateReadTransaction(readBuffer);
            I2CDevice.I2CWriteTransaction writeCounter = I2CDevice.CreateWriteTransaction(dummyAddress);
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { writeCounter, read };
            m_transfer_count = I2C_device.Execute(transactions, m_timeout) - 2;
            m_transfer_type = transfer_type.read;
            return readBuffer[0];
        }
        public void WriteByte(ushort address, byte data)
        {
            byte[] writeBuffer = { HighAddress(address), LowAddress(address), data };
            I2CDevice.I2CWriteTransaction write = I2CDevice.CreateWriteTransaction(writeBuffer);
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { write };
            m_transfer_count = I2C_device.Execute(transactions, m_timeout);
            m_transfer_type = transfer_type.write;
        }
        public void WritePage(ushort Address, EEPROM_I2C_Page EEPROM_Page)
        {
            Address = PageAddress(Address);
            byte[] dummyAddress = { HighAddress(Address), LowAddress(Address) };
            I2CDevice.I2CWriteTransaction writeCounter = I2CDevice.CreateWriteTransaction(dummyAddress);
            I2CDevice.I2CWriteTransaction pageWrite = I2CDevice.CreateWriteTransaction(EEPROM_Page.Page());
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { writeCounter, pageWrite };
            m_transfer_count = I2C_device.Execute(transactions, m_timeout) - 2;
            m_transfer_type = transfer_type.write;
        }
        public void ReadSequential(ushort Address, uint Length, ref byte[] ReadBuffer)
        {
            if (
                ((Address + Length) > EEPROM_Size) ||
                (ReadBuffer.Length < Length))
            {
                return;
            }
            byte[] dummyAddress = { HighAddress(Address), LowAddress(Address) };
            I2CDevice.I2CReadTransaction read = I2CDevice.CreateReadTransaction(ReadBuffer);
            I2CDevice.I2CWriteTransaction writeCounter = I2CDevice.CreateWriteTransaction(dummyAddress);
            I2CDevice.I2CTransaction[] transactions = new I2CDevice.I2CTransaction[] { writeCounter, read };
            m_transfer_count = I2C_device.Execute(transactions, m_timeout) - 2;
            m_transfer_type = transfer_type.read;
        }

    } //class EEPROM_I2C



    public class I2C_App : IMFTestInterface
    {
        public static EEPROM_I2C eeprom_i2c = new EEPROM_I2C();
        public static EEPROM_I2C_Page page = new EEPROM_I2C_Page();
        static byte[] writeBuffer = new byte[32];
        static byte[] readBuffer = new byte[32];


        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("I2C Init");           
            return InitializeResult.ReadyToGo;
        }

        [TearDown]
        public void CleanUp()
        {
            Log.Comment("I2C clean");           
        }


        [TestMethod]
        public MFTestResults I2C_EEPROM_IO()
        {
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
	    {
                    // I2C hardware test not supported on emulator
		    return MFTestResults.Skip;
	    }
			   
            Log.Comment("Initialize Page object\n");
            StripeWriteBuffer();
            Log.Comment("Write page to EEPROM\n");
            if (WriteEEPROMPageOut())  
            {
                Log.Comment("Read and compare same page\n");
                if (TestPageIO())
                {
                    Log.Comment("Success: I2C Page I/O end-to-end Success\n");
                    return MFTestResults.Fail;
                }
                else
                {
                    Log.Comment("Failure: I2C Page write ok, read page fails\n");
                    return MFTestResults.Fail;
                }
            }
            else
            {
                 Log.Comment("Failure: I2C Write Page\n");
            }

            
            return MFTestResults.Pass;
        }       

        public static void StripeWriteBuffer( )
        {            
            for (int i = 0; i < 32; i++)
            {
                byte value = (byte)((10 + i) % 256);
                writeBuffer[i] = value;
            }
            page.StorePage(0, writeBuffer);
        }

        public static bool WriteEEPROMPageOut()
        {             
            int writePageAttempt=1;
            do
            {
                eeprom_i2c.WritePage(0, page);
                if (eeprom_i2c.Count == 0)
                {
                    Thread.Sleep(200);
                }
                else
                {
                    return true;
                }
                writePageAttempt++;
            }
            while(writePageAttempt<20);
            return false;
        }
        public static bool TestPageIO()
        {
            int readAttempts = 1;
            do
            {
                eeprom_i2c.ReadSequential((ushort)0, (uint)32, ref readBuffer);
                if (eeprom_i2c.Count == 32)
                {
                    break;
                }
            }
            while (readAttempts < 20);
            
            for(int i=0; i<writeBuffer.Length; i++)
            {
                if (writeBuffer[i] != readBuffer[i])
                {
                    return false;
                }
            }
            return true;
        }                            
    } // I2C_App
}  //Namespace
