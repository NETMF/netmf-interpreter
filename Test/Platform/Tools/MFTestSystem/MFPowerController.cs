using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace Microsoft.SPOT.Platform.Test
{
    class MFPowerController
    {
        #region Enumerations

        internal enum PowerSource
        {
            DC,
            USB,
            All
        }

        internal enum PowerState
        {
            On,
            Off
        }

        #endregion

        #region Reset

        /// <summary>
        /// Call this method to reset the device.
        /// </summary>
        internal static void Reset()
        {
            Switch(PowerState.Off, PowerSource.All);
            Switch(PowerState.On, PowerSource.All);
        }

        /// <summary>        
        /// Sending a 0 to the comport will toggle the DC power supply.
        /// Sending a 1 to the comport will toggle the usb power supply.
        /// Sending a 255 to the comport will give you current status of the device.
        /// The status of the device can be one of the four below:
        ///         0x0 = DC & USB off
        ///         0x1 = DC on
        ///         0x2 = USB on
        ///         0x3 = DC & USB on        
        /// </summary>
        /// <param name="power">This parameter specifies the power state which can be on or off.</param>
        /// <param name="ps">This parameter specifies the power source.</param>
        internal static void Switch(PowerState power, PowerSource ps)
        {
            byte status;
            switch (GetStatus())
            {
                case 0: // Both DC & USB off.
                    if (power == PowerState.Off)
                    {
                        return;
                    }
                    else
                    {
                        switch ((int)ps)
                        {
                            case 2:
                                status = Send(0);
                                CheckStatus(status, 0);
                                status = Send(1);
                                CheckStatus(status, 1);
                                break;
                            default:
                                byte b = Convert.ToByte((int)ps);
                                status = Send(b);
                                CheckStatus(status, b);
                                break;
                        }
                    }
                    break;
                case 1: // Only DC is on.
                    if (power == PowerState.Off)
                    {
                        switch ((int)ps)
                        {
                            case 0:
                                status = Send(0);
                                CheckStatus(status, 0);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)ps)
                        {
                            case 0:
                                break;
                            default:
                                status = Send(1);
                                CheckStatus(status, 1);
                                break;
                        }
                    }
                    break;
                case 2: // Only USB is on.
                    if (power == PowerState.Off)
                    {
                        switch ((int)ps)
                        {
                            case 1:
                                status = Send(1);
                                CheckStatus(status, 1);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        switch ((int)ps)
                        {
                            case 1:
                                break;
                            default:
                                status = Send(0);
                                CheckStatus(status, 0);
                                break;
                        }
                    }
                    break;
                case 3: // Both DC & USB on.
                    if (power == PowerState.Off)
                    {
                        switch ((int)ps)
                        {
                            case 2:
                                status = Send(0);
                                CheckStatus(status, 0);
                                status = Send(1);
                                CheckStatus(status, 1);
                                break;
                            default:
                                byte b = Convert.ToByte((int)ps);
                                status = Send(b);
                                CheckStatus(status, b);
                                break;
                        }
                    }
                    else
                    {
                        return;
                    }
                    break;
            }
        }

        #endregion

        #region Private Methods

        private static void CheckStatus(byte actualStatus, byte expectedStatus)
        {
            if (actualStatus != expectedStatus)
            {
                string stat = string.Format("Expected status: {0}\n Actual status: {1}",
                    actualStatus, expectedStatus);
                throw new ApplicationException(stat);
            }
        }

        private static byte GetStatus()
        {
            int attempts = 0;
            byte status;

            while (true)
            {
                try
                {
                    // Send 255 to know device status.
                    status = Send(255);
                }
                catch
                {
                    if (attempts++ < 3)
                    {
                        continue;
                    }
                    else
                    {
                        throw new ApplicationException(
                            "Serial Port Communication Error: Please verify your device is connected.");
                    }
                }
                break;
            }

            return status;
        }

        private static byte Send(byte b)
        {
            using (SerialPort comport = new SerialPort("COM1", 9600))
            {
                int count = 0;
                bool readTimedOut = false;
                byte[] buffer = new byte[1];
                buffer[0] = b;

                comport.Open();
                comport.Write(buffer, 0, 1);
                comport.ReadTimeout = 1000;

                try
                {
                    count = comport.Read(buffer, 0, 1);
                }
                catch (TimeoutException)
                {
                    readTimedOut = true;
                }

                if (count != 1 && !readTimedOut)
                {
                    throw new Exception("0 bytes read");
                }

                comport.Close();

                return buffer[0];
            }
        }

        #endregion
    }
}
