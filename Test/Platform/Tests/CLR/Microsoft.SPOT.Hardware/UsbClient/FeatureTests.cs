////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Microsoft Corporation.  All rights reserved.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Platform.Test;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Hardware.UsbClient;

namespace Microsoft.SPOT.Platform.Tests
{
    public class USB
    {
        public UsbController[] controllers;
        
        public USB()
        {
            controllers = UsbController.GetControllers();
        }
        public void DeviceDescriptor(Configuration.DeviceDescriptor descriptor)
        {
            string str = "DeviceDescriptor\n";
            if (descriptor.bcdDevice == 0x01)
            {
            }
            str += "bcdUSB:" + descriptor.bcdUSB.ToString() + "\n";
            str += "deviceClass:" + descriptor.bDeviceClass.ToString() + "\n";
            str += "productID:" + descriptor.idProduct.ToString() + "\n";
            str += "vendorID:" + descriptor.idVendor.ToString() + "\n";

        } //DeviceDescriptor

        public void ConfigurationDescriptor(Configuration.ConfigurationDescriptor descriptor)
        {
            if (descriptor.interfaces.Length == 0)
            {
                Log.Exception("0 interfaces found - non-zero is expected");
            }
            else
            {
                //Configuration.UsbInterface[] UsbInterface = descriptor.interfaces;
                Log.Comment("Found " + descriptor.interfaces.Length.ToString() + " interfaces");
                foreach (Configuration.UsbInterface UsbInterface in descriptor.interfaces)
                {
                    if (UsbInterface.endpoints.Length == 0)
                    {
                        Log.Comment("Found no endpoints - ok IFF endpoint 0 is exclusive");
                    }
                    else
                    {
                        Log.Comment("Found " + UsbInterface.endpoints.Length.ToString() + "endpoints");
                    }
                }
            } //if interfaces         
        } //ConfigurationDescriptor

        public void ParseDescriptor(Configuration.Descriptor descriptor)
        {
            Type descriptorType = descriptor.GetType();
            if (descriptorType == typeof(Configuration.DeviceDescriptor))
            {
                DeviceDescriptor((Configuration.DeviceDescriptor) descriptor);
                return;
            }
            if (descriptorType == typeof(Configuration.ConfigurationDescriptor))
            {
                ConfigurationDescriptor((Configuration.ConfigurationDescriptor)descriptor);
                return;
            }
            
            Log.Comment("descriptor not parsed " + descriptor.ToString());

        } //ParseDescriptor

    } //USB Class

    public class UsbClient : IMFTestInterface
    {
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
            Log.Comment("UsbClient CleanUp");           
        }

        
        [TestMethod]
        public MFTestResults TestUSB()
        {
            USB usbDevices=null;
            try
            {
                usbDevices = new USB();
            }
            catch (System.NotSupportedException)
            {
                if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)  //The Emulator on Windows Host
                {
                    Log.Comment("UsbClient not supported UnSupported Exception expected");
                    return MFTestResults.KnownFailure;
                }
            }
            catch (Exception E)
            {
                Log.Exception("UsbClient Unexpected usbDevices enumeration failed" + E.Message);
                return MFTestResults.Fail;
            }
            //
            // usbDevices enumerated with something, now check known issue
            //
            if (Microsoft.SPOT.Hardware.SystemInfo.SystemID.SKU == 3)
            {
                Log.Comment("UsbClient does not return expected NotSupprotedException on Windows - bug 20936\n" );
                return MFTestResults.KnownFailure;
            }
           
            if (usbDevices.controllers.Length == 0)
            {
                //
                // Note: For platforms without USB controllers
                //
                Log.Comment("Device Platform has 0 USB Controllers");
                return MFTestResults.Skip;
            }
            
            try
            {
                return TestDescriptors(usbDevices);
            }
            catch (Exception e)
            {
                Log.Exception("UsbClient unknown failure - TestDescriptors" + e.Message + e.StackTrace);
                return MFTestResults.Fail;                
            }
        } //TestUSB

       
        private MFTestResults TestDescriptors(USB USB1)
        {
            Log.Comment("Found " + USB1.controllers.Length.ToString() + " controller(s)");

            foreach(UsbController UsbController in USB1.controllers)
            {
                Configuration UsbConfig = UsbController.Configuration;
                Log.Comment("Number of descriptor elements = " + UsbConfig.descriptors.Length.ToString());
                foreach (Configuration.Descriptor descriptor in UsbConfig.descriptors)
                {
                    USB1.ParseDescriptor(descriptor);
                }
         
                try
                {
                        // As a test, set the same configuration back to see if its data matches
                    ;//USB1.controllers[0].Configuration = UsbConfig;
                }
                catch(ArgumentException)
                {
                    Log.Exception("USB configuration not set - error " + 
                        UsbController.ConfigurationError.ToString());
                    return MFTestResults.Fail;
                }
            } //UsbController
            return MFTestResults.Pass;
        } //TestDescriptors
    }
}
