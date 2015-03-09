////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using Microsoft.SPOT.Hardware.UsbClient;
using System.Collections;
namespace Microsoft.SPOT.Platform.Tests
{
    public class EmulateHardware : IMFTestInterface
    {
        ArrayList pinObjectList = new ArrayList();
        public EmulateHardware()
        {
            pinObjectList.Add(Cpu.Pin.GPIO_NONE);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin0);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin1);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin2);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin3);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin4);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin5);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin6);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin7);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin8);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin9);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin10);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin11);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin12);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin13);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin14);
            pinObjectList.Add(Cpu.Pin.GPIO_Pin15);          
        }

        public class HardwareClassList : IDisposable
        {
            private bool disposed=false;
            public Battery.ChargerModel myCharger = null;
            //
            //skipping static objects because 
            // test is focused on initiation and dispose 
            //
            //public static Cpu myCPU;
            //public Cpu.Pin myPin = null;
            //public Cpu.PinUsage myPinUsage = null;
            //public Cpu.PinValidInterruptMode myPinInterruptMode = null;
            //public Cpu.PinValidResistorMode myPinValidResistorMode=null;

            public HardwareProvider myHardwareProvider = null;
            public I2CDevice myI2CDevice = null;
            public I2CDevice.Configuration myI2CDeviceConfiguration = null;
            public I2CDevice.I2CReadTransaction myI2CDeviceRead = null;
            public I2CDevice.I2CTransaction myI2CDeviceTrans = null;
            public I2CDevice.I2CWriteTransaction myI2CDeviceWrite = null;
            public InputPort myInputPort = null;
            public InterruptPort myInterruptPort = null;
            
            public OutputPort myOutputPort = null;
            
            public SPI mySPI = null;
            public SPI.Configuration mySPIConfiguration = null;
            public TristatePort myTristatePort = null;

            public Serial mySerial = null;
            public SerialPort mySerialPort = null;
 
            public  void InitializeBatteryClass()
            {
                //myCharger = new Battery.ChargerModel();
            }             

            public void InitializePortClass()
            {
                
                myInputPort = new InputPort(Cpu.Pin.GPIO_Pin0,
                                             false,
                                             Port.ResistorMode.Disabled);

                myInterruptPort = new InterruptPort(Cpu.Pin.GPIO_Pin1,
                                                    false,
                                                    Port.ResistorMode.Disabled,
                                                    Port.InterruptMode.InterruptEdgeBoth);

                myOutputPort = new OutputPort(Cpu.Pin.GPIO_Pin12, false);



                myTristatePort = new TristatePort(Cpu.Pin.GPIO_Pin2,
                                                    false,
                                                    false,
                                                    Port.ResistorMode.Disabled);
            }

            public void InitializeSPI()
            {
                mySPIConfiguration = new SPI.Configuration(Cpu.Pin.GPIO_Pin10,
                                                           false,
                                                           100,
                                                           10,
                                                           false,
                                                           false,
                                                           400,
                                                           SPI.SPI_module.SPI1);
                mySPI = new SPI(mySPIConfiguration);
            }

            public void InitializeI2C()
            {
                myI2CDeviceConfiguration = new I2CDevice.Configuration(0x0, 0x0);
                myI2CDevice = new I2CDevice(myI2CDeviceConfiguration);
#if false
                myI2CDeviceRead = new  I2CDevice.I2CReadTransaction( );
                myI2CDeviceTrans = new  I2CDevice.I2CTransaction();
                myI2CDeviceWrite = new  I2CDevice.I2CWriteTransaction();
#endif
            }

            public void InitializeHWProvider()
            {
                myHardwareProvider = new HardwareProvider();             
            }

            public void InitializeSerial()
            {
                mySerialPort = new SerialPort(Serial.COM1);
            }

            private void Dispose(Boolean disposing)
            {
                if (!this.disposed)
                {
                    Log.Comment("disposing classes");                   
                }
                disposed=true;
            }

            public void Dispose()
            {              
                myInputPort.Dispose();
                myInterruptPort.Dispose();
                myOutputPort.Dispose();
                myI2CDevice.Dispose(); 
                mySPI.Dispose();
                myTristatePort.Dispose();             
                mySerialPort.Dispose();
                Dispose(true);
                GC.SuppressFinalize(this);
            }
           
            ~HardwareClassList()
            {
                Log.Comment(" HardwareClassList destructor running");
            }
        } //Hardware



        [SetUp]
        public InitializeResult Initialize()
        {
            Log.Comment("Adding set up for the tests.");
            // Add your functionality here.   
            
            return InitializeResult.ReadyToGo;
        }
        [TearDown]
        public void CleanUp()
        {
        }

        [TestMethod]
        public MFTestResults HWResourceTest()
        {
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)  //The Emulator on Windows Host
            {
                Log.Comment("Emulate Hardware test is restricted to emulator");
                return MFTestResults.Skip;
            }
            else
            {
                Log.Comment("EmulateHardware Resource Test Starting");
            }
            Cpu.Pin mask;
            Cpu.Pin miso;
            Cpu.Pin mosi;
            Cpu.Pin rxPin;
            Cpu.Pin txPin;
            Cpu.Pin rtsPin;
            Cpu.Pin ctsPin;

            int length;
            int width;
            int bitPerPixel;
            int orientationDeg;

            try
            {
                HardwareProvider.HwProvider.GetSpiPins(SPI.SPI_module.SPI1,
                    out mask,
                    out miso,
                    out mosi);
                Log.Comment("SPI mask pin: " + mask.ToString());
                Log.Comment("SPI miso pin: " + miso.ToString());
                Log.Comment("SPI mosi pin: " + mosi.ToString());

                HardwareProvider.HwProvider.GetLCDMetrics(
                    out length,
                    out width,
                    out bitPerPixel,
                    out orientationDeg);
            }
            catch(Exception E)
            {
                Log.Comment("Pin resource failure" + E.Message);
                return MFTestResults.Fail;
            }
            try
            {
                int count = HardwareProvider.HwProvider.GetSerialPortsCount();
                if (count > 0)
                {
                    HardwareProvider.HwProvider.GetSerialPins(
                            "COM1",
                            out rxPin,
                            out txPin,
                            out ctsPin,
                            out rtsPin);
                    Log.Comment("Serial rx pin: " + rxPin.ToString());
                    Log.Comment("Serial tx pin: " + txPin.ToString());
                    Log.Comment("Serial rts pin: " + rtsPin.ToString());
                    Log.Comment("Serial cts pin: " + ctsPin.ToString());
                }
            }
            catch (Exception E)
            {
                Log.Comment("Bug 20982 serial port enumeration begins with COM0 and should be COM1");
                Log.Comment(E.Message + E.StackTrace);
                return MFTestResults.KnownFailure;

            }
            return MFTestResults.Pass;
        }
        
        [TestMethod]
        public MFTestResults HWObjectCreate()
        {

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)  //The Emulator on Windows Host
            {
                Log.Comment("Emulate Hardware test is restricted to emulator");
                return MFTestResults.Skip;
            }
            else
            {
                Log.Comment("EmulateHardware Object Test Starting");            
            }

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)
            {
                Log.Comment(" Emulator not detected - skipping");
                return MFTestResults.Skip;
            }
            HardwareClassList HW = new HardwareClassList();
            try
            {
                Log.Comment("Init hardware provider ");
                HW.InitializeHWProvider();
            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("Init Battery Class ");
                HW.InitializeBatteryClass();
            }

            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("Init Port Class");
                HW.InitializePortClass();
            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }

            try
            {
                Log.Comment("Init SPI Class");
                HW.InitializeSPI();
            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("Init I2C Class");
                HW.InitializeI2C();
            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }
            try
            {
                Log.Comment("Init Serial Port");
                HW.InitializeSerial();
            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;
            }  
            try
            {
                Log.Comment("All HW classes created, now to be disposed");
                HW.Dispose();

            }
            catch (Exception E)
            {
                Log.Comment(E.Message);
                return MFTestResults.Fail;

            }
            return MFTestResults.Pass;
        } //HWObjectCreate

        [TestMethod]
        public MFTestResults HWObjectPinTest()
        {
            int i;

            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU != 3)  //The Emulator on Windows Host
            {
                Log.Comment("Emulate Hardware test is restricted to emulator");
                return MFTestResults.Skip;
            }
            else
            {
                Log.Comment("EmulateHardware Pin Test Starting");
            }
            ArrayList portObjectList = new ArrayList();        
            try
            {
                Log.Comment("Assign all pins to OutputPort");
               
                for(i = 0; i < pinObjectList.Count; ++i)
                {
                    Cpu.Pin pin = (Cpu.Pin)pinObjectList[i];

                    if (pin.Equals(Cpu.Pin.GPIO_NONE))
                    {
                        Log.Comment("Skip creation of port with GPIO_NONE throws exception. Tested below in this function");
                        continue;
                    }
                    
                    portObjectList.Add(new OutputPort(pin, false));
                }

                
            }
            catch (Exception E)
            {
                   string str =  "Unable to create output port object with pin " + 
                    E.Message;
                Log.Comment(str);
                
                return MFTestResults.Fail;
            }

            // Test that creation  of port with GPIO_NONE throws exception.

            try
            {
                Log.Comment("Creating Output port with Cpu.Pin.GPIO_NONE. Should through exception");
                portObjectList.Add(new OutputPort(Cpu.Pin.GPIO_NONE, false) );
 
            }
            catch (Exception E)
            {
                Log.Comment("Catching exception as result of creation OutputPort with Cpu.Pin.GPIO_NONE" + E.Message);
            }

            int exceptionCount=0;
            
            for(i = 0; i < pinObjectList.Count; ++i)
            {
                Cpu.Pin pin = (Cpu.Pin)pinObjectList[i];

                try
                {
                    portObjectList.Add(new OutputPort(pin, false));
                    Log.Comment("Using pin " + pin.ToString() + " in object instance unexpected success");
                }
                catch
                {
                    exceptionCount++;
                }
            }
            if (exceptionCount != (pinObjectList.Count))
            {
                Log.Comment("A pin reservation succeeded and expected to fail");
                return MFTestResults.Fail;
            }

            return MFTestResults.Pass;
        } //HWObjectDispose
    }
}
