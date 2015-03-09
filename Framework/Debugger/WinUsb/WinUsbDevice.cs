//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// This file is part of the Microsoft .NET Micro Framework Porting Kit Code Samples and is unsupported. 
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use these files except in compliance with the License.
// You may obtain a copy of the License at:
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing
// permissions and limitations under the License.
// 
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace WinUsb
{
    /// <summary>
    ///  Routines for the WinUsb driver supported by Windows Vista and Windows XP.
    ///  </summary>
    ///  
    sealed public class WinUsbDevice
    {
        const UInt32 PipeReadTimeout = 500;
        const UInt32 PipeWriteTimeout = 500;

        public struct devInfo
        {
            public SafeFileHandle deviceHandle;
            internal IntPtr winUsbHandle;
            public Byte bulkInPipe;
            public Byte bulkOutPipe;
            public Byte interruptInPipe;
            public Byte interruptOutPipe;
            public UInt32 devicespeed;
        }

        public devInfo DeviceInfo = new devInfo();

        public bool ReadPipe( Byte pipeId
                            , Byte[ ] buffer
                            , UInt32 bufferLength
                            , ref UInt32 lengthTransferred
                            , IntPtr overlapped
                            )
        {
            return NativeMethods.WinUsb_ReadPipe( DeviceInfo.winUsbHandle, pipeId, buffer, bufferLength, ref lengthTransferred, overlapped );
        }

        public bool WritePipe( Byte pipeId
                             , Byte[ ] buffer
                             , UInt32 bufferLength
                             , ref UInt32 lengthTransferred
                             , IntPtr overlapped
                             )
        {
            return NativeMethods.WinUsb_WritePipe( DeviceInfo.winUsbHandle, pipeId, buffer, bufferLength, ref lengthTransferred, overlapped );
        }

        ///  <summary>
        ///  Closes the device handle obtained with CreateFile and frees resources.
        ///  </summary>
        ///  
        public void CloseDeviceHandle()
        {
            if (DeviceInfo.winUsbHandle != IntPtr.Zero)
            {
                IntPtr h = DeviceInfo.winUsbHandle;
                NativeMethods.WinUsb_Free(h);
                DeviceInfo.winUsbHandle = IntPtr.Zero;
            }

            if (!(DeviceInfo.deviceHandle == null))
            {
                if (!(DeviceInfo.deviceHandle.IsInvalid))
                {
                    DeviceInfo.deviceHandle.Close();
                    DeviceInfo.deviceHandle.SetHandleAsInvalid();
                } 
                DeviceInfo.deviceHandle = null;
            }
        }

        ///  <summary>
        ///  Initiates a Control Read transfer. Data stage is device to host.
        ///  </summary>
        /// 
        ///  <param name="dataStage"> The received data. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        public int Do_Control_Read_Transfer(ref Byte[] dataStage, byte request, ushort value) 
        {
            UInt32 bytesReturned = 0;
            NativeMethods.WINUSB_SETUP_PACKET setupPacket;
            Boolean success;

            try
            {
                //  Vendor-specific request to an interface with device-to-host Data stage.

                setupPacket.RequestType = 0XC1;

                //  The request number that identifies the specific request.

                setupPacket.Request = request;

                //  Command-specific value to send to the device.

                setupPacket.Value = value;

                //  Command-specific value to send to the device.

                setupPacket.Index = 0;

                //  Number of bytes in the request's Data stage.

                setupPacket.Length = System.Convert.ToUInt16(dataStage.Length);

                // ***
                //  winusb function 

                //  summary
                //  Initiates a control transfer.

                //  paramaters
                //  Device handle returned by WinUsb_Initialize.
                //  WINUSB_SETUP_PACKET structure 
                //  Buffer to hold the returned Data-stage data.
                //  Number of data bytes to read in the Data stage.
                //  Number of bytes read in the Data stage.
                //  Null pointer for non-overlapped.

                //  returns
                //  True on success.
                //  ***            

                success = NativeMethods.WinUsb_ControlTransfer( DeviceInfo.winUsbHandle, setupPacket, dataStage, System.Convert.ToUInt16( dataStage.Length ), ref bytesReturned, IntPtr.Zero );
                return bytesReturned == 0 ? -1 : (int)bytesReturned;
            }
            catch
            {
                throw;
            }
        }

        ///  <summary>
        ///  Is the endpoint's direction IN (device to host)?
        ///  </summary>
        ///  
        ///  <param name="addr"> The endpoint address. </param>
        ///  <returns>
        ///  True if IN (device to host), False if OUT (host to device)
        ///  </returns> 

        ///  <summary>
        ///  Initializes a device interface and obtains information about it.
        ///  Calls these winusb API functions:
        ///    WinUsb_Initialize
        ///    WinUsb_QueryInterfaceSettings
        ///    WinUsb_QueryPipe
        ///  </summary>
        ///  
        ///  <param name="deviceHandle"> A handle obtained in a call to winusb_initialize. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>

        public Boolean InitializeDevice()
        {
            NativeMethods.USB_INTERFACE_DESCRIPTOR ifaceDescriptor;
            NativeMethods.WINUSB_PIPE_INFORMATION pipeInfo;
            UInt32 pipeTimeout = 2000;
            Boolean success;

            try
            {
                ifaceDescriptor.bLength = 0;
                ifaceDescriptor.bDescriptorType = 0;
                ifaceDescriptor.bInterfaceNumber = 0;
                ifaceDescriptor.bAlternateSetting = 0;
                ifaceDescriptor.bNumEndpoints = 0;
                ifaceDescriptor.bInterfaceClass = 0;
                ifaceDescriptor.bInterfaceSubClass = 0;
                ifaceDescriptor.bInterfaceProtocol = 0;
                ifaceDescriptor.iInterface = 0;

                pipeInfo.PipeType = 0;
                pipeInfo.PipeId = 0;
                pipeInfo.MaximumPacketSize = 0;
                pipeInfo.Interval = 0;

                // ***
                //  winusb function 

                //  summary
                //  get a handle for communications with a winusb device        '

                //  parameters
                //  Handle returned by CreateFile.
                //  Device handle to be returned.

                //  returns
                //  True on success.
                //  ***

                success = NativeMethods.WinUsb_Initialize
                    (DeviceInfo.deviceHandle,
                    ref DeviceInfo.winUsbHandle);

                if (success)
                {
                    // ***
                    //  winusb function 

                    //  summary
                    //  Get a structure with information about the device interface.

                    //  parameters
                    //  handle returned by WinUsb_Initialize
                    //  alternate interface setting number
                    //  USB_INTERFACE_DESCRIPTOR structure to be returned.

                    //  returns
                    //  True on success.

                    success = NativeMethods.WinUsb_QueryInterfaceSettings
                        (DeviceInfo.winUsbHandle,
                        0,
                        ref ifaceDescriptor);

                    if (success)
                    {
                        //  Get the transfer type, endpoint number, and direction for the interface's
                        //  bulk and interrupt endpoints. Set pipe policies.

                        // ***
                        //  winusb function 

                        //  summary
                        //  returns information about a USB pipe (endpoint address)

                        //  parameters
                        //  Handle returned by WinUsb_Initialize
                        //  Alternate interface setting number
                        //  Number of an endpoint address associated with the interface. 
                        //  (The values count up from zero and are NOT the same as the endpoint address
                        //  in the endpoint descriptor.)
                        //  WINUSB_PIPE_INFORMATION structure to be returned

                        //  returns
                        //  True on success   
                        // ***

                        for (Int32 i = 0; i <= ifaceDescriptor.bNumEndpoints - 1; i++)
                        {
                            NativeMethods.WinUsb_QueryPipe
                                (DeviceInfo.winUsbHandle,
                                0,
                                System.Convert.ToByte(i),
                                ref pipeInfo);

                            if (((pipeInfo.PipeType ==
                                NativeMethods.USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                UsbEndpointDirectionIn(pipeInfo.PipeId)))
                            {
                                DeviceInfo.bulkInPipe = pipeInfo.PipeId;

                                SetPipePolicy
                                    (DeviceInfo.bulkInPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.IGNORE_SHORT_PACKETS),
                                    Convert.ToByte(false));

                                SetPipePolicy
                                    (DeviceInfo.bulkInPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.PIPE_TRANSFER_TIMEOUT),
                                    pipeTimeout);

                            }
                            else if (((pipeInfo.PipeType ==
                                NativeMethods.USBD_PIPE_TYPE.UsbdPipeTypeBulk) &
                                UsbEndpointDirectionOut(pipeInfo.PipeId)))
                            {

                                DeviceInfo.bulkOutPipe = pipeInfo.PipeId;

                                SetPipePolicy
                                    (DeviceInfo.bulkOutPipe,
                                    Convert.ToUInt32( NativeMethods.POLICY_TYPE.IGNORE_SHORT_PACKETS ),
                                    Convert.ToByte(false));

                                SetPipePolicy
                                    (DeviceInfo.bulkOutPipe,
                                    Convert.ToUInt32( NativeMethods.POLICY_TYPE.PIPE_TRANSFER_TIMEOUT ),
                                    pipeTimeout);

                            }
                            else if ((pipeInfo.PipeType ==
                                NativeMethods.USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                UsbEndpointDirectionIn(pipeInfo.PipeId))
                            {

                                DeviceInfo.interruptInPipe = pipeInfo.PipeId;

                                SetPipePolicy
                                    (DeviceInfo.interruptInPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.IGNORE_SHORT_PACKETS),
                                    Convert.ToByte(false));

                                SetPipePolicy
                                    (DeviceInfo.interruptInPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.PIPE_TRANSFER_TIMEOUT),
                                    pipeTimeout);

                            }
                            else if ((pipeInfo.PipeType ==
                                NativeMethods.USBD_PIPE_TYPE.UsbdPipeTypeInterrupt) &
                                UsbEndpointDirectionOut(pipeInfo.PipeId))
                            {

                                DeviceInfo.interruptOutPipe = pipeInfo.PipeId;

                                SetPipePolicy
                                    (DeviceInfo.interruptOutPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.IGNORE_SHORT_PACKETS),
                                    Convert.ToByte(false));

                                SetPipePolicy
                                    (DeviceInfo.interruptOutPipe,
                                    Convert.ToUInt32(NativeMethods.POLICY_TYPE.PIPE_TRANSFER_TIMEOUT),
                                    pipeTimeout);
                            }
                        }
                    }
                    else
                    {
                        success = false;
                    }
                }
                return success;
            }
            catch
            {
                throw;
            }
        }

        ///  <summary>
        ///  Sets pipe policy.
        ///  Used when the value parameter is a Byte (all except PIPE_TRANSFER_TIMEOUT).
        ///  </summary>
        ///  
        ///  <param name="pipeId"> Pipe to set a policy for. </param>
        ///  <param name="policyType"> POLICY_TYPE member. </param>
        ///  <param name="value"> Policy value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>
        ///  
        private Boolean SetPipePolicy(Byte pipeId, UInt32 policyType, Byte value)
        {
            Boolean success;

            try
            {
                // ***
                //  winusb function 

                //  summary
                //  sets a pipe policy 

                //  parameters
                //  handle returned by WinUsb_Initialize
                //  identifies the pipe
                //  POLICY_TYPE member.
                //  length of value in bytes
                //  value to set for the policy.

                //  returns
                //  True on success 
                // ***

                success = NativeMethods.WinUsb_SetPipePolicy
                    (DeviceInfo.winUsbHandle,
                    pipeId,
                    policyType,
                    1,
                    ref value);

                return success;
            }
            catch
            {
                throw;
            }
        }

        ///  <summary>
        ///  Sets pipe policy.
        ///  Used when the value parameter is a UInt32 (PIPE_TRANSFER_TIMEOUT only).
        ///  </summary>
        ///  
        ///  <param name="pipeId"> Pipe to set a policy for. </param>
        ///  <param name="policyType"> POLICY_TYPE member. </param>
        ///  <param name="value"> Policy value. </param>
        ///  
        ///  <returns>
        ///  True on success, False on failure.
        ///  </returns>
        ///  
        private Boolean SetPipePolicy(Byte pipeId, UInt32 policyType, UInt32 value)
        {
            Boolean success;

            try
            {
                // ***
                //  winusb function 

                //  summary
                //  sets a pipe policy 

                //  parameters
                //  handle returned by WinUsb_Initialize
                //  identifies the pipe
                //  POLICY_TYPE member.
                //  length of value in bytes
                //  value to set for the policy.

                //  returns
                //  True on success 
                // ***

                success = NativeMethods.WinUsb_SetPipePolicy1
                    (DeviceInfo.winUsbHandle,
                    pipeId,
                    policyType,
                    4,
                    ref value);

                return success;
            }
            catch
            {
                throw;
            }
        }

        ///  <summary>
        ///  Is the endpoint's direction IN (device to host)?
        ///  </summary>
        ///  
        ///  <param name="addr"> The endpoint address. </param>
        ///  <returns>
        ///  True if IN (device to host), False if OUT (host to device)
        ///  </returns> 

        private Boolean UsbEndpointDirectionIn(Int32 addr)
        {
            Boolean directionIn;

            try
            {
                if (((addr & 0X80) == 0X80))
                {
                    directionIn = true;
                }
                else
                {
                    directionIn = false;
                }

            }
            catch
            {
                throw;
            }
            return directionIn;
        }

        ///  <summary>
        ///  Is the endpoint's direction OUT (host to device)?
        ///  </summary>
        ///  
        ///  <param name="addr"> The endpoint address. </param>
        ///  
        ///  <returns>
        ///  True if OUT (host to device, False if IN (device to host)
        ///  </returns>

        private Boolean UsbEndpointDirectionOut(Int32 addr)
        {
            Boolean directionOut;

            try
            {
                if (((addr & 0X80) == 0))
                {
                    directionOut = true;
                }
                else
                {
                    directionOut = false;
                }
            }
            catch
            {
                throw;
            }
            return directionOut;
        }
    }
}
