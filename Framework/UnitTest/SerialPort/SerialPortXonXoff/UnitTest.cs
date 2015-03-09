using System;
using System.Threading;
using System.Collections;
using System.Runtime.CompilerServices;

using Microsoft.SPOT.Hardware;
using System.IO.Ports;


namespace Microsoft.SPOT.UnitTest.SerialPortUnitTest
{
    public class SerialXonXoff
    {
        static void Main(string[] args)
        {   
            SerialPort ser = new SerialPort( "COM1", (int)BaudRate.Baudrate115200 );
            byte[] buf = new byte[26];
            byte[] ret = new byte[26];

            ser.Handshake = Handshake.XOnXOff;
            ser.DataReceived += new SerialDataReceivedEventHandler(ser_DataReceived);

            ser.Open();

            Debug.Print("Initializing Serial Test");
                
            for (int i = 0;i < buf.Length;i++)
            {
                buf[i] = (byte)('a' + i);
            }

            ser.Write(buf, 0, buf.Length);
            ser.Flush();

            Thread.Sleep(1000);

            ser.DataReceived -= new SerialDataReceivedEventHandler(ser_DataReceived);

            ser.Write(buf, 0, buf.Length);

            Thread.Sleep(1000);

            ser.DataReceived += new SerialDataReceivedEventHandler(ser_DataReceived);
            ser.ErrorReceived += new SerialErrorReceivedEventHandler(ser_ErrorReceived);

            ser.Write(buf, 0, buf.Length);

            Thread.Sleep(3000);

            ser.Read(ret, 0, ret.Length);

            Thread.Sleep(10000);             
        }

        static void ser_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Print("Error Received");
        }

        static void ser_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Debug.Print("Data Received");
        }
    }
}
