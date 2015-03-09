using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using System.IO.Ports;
using Microsoft.SPOT.Input;

namespace Microsoft.SPOT.HardwareProviderUnitTest
{

    /// <summary>
    /// Reads from the Interop function of the hardware provider
    /// </summary>
    public class GPIOPortInteropTest
    {

        public static void Main(string[] args)
        {
                HardwareProvider hwProvider = HardwareProvider.HwProvider;

                Debug.Print("Test GPIO Hardware provider functions ");

                while (true)
               {
                // GPIO
                int GpioCnt = hwProvider.GetPinsCount();
                Cpu.PinUsage[] PinMap;
                int size;
                hwProvider.GetPinsMap(out PinMap, out size);
                Debug.Print ("------------------------------");
                
                Debug.Print ("-------Pin Map ----------");
                for (int i = 0; i < GpioCnt; i++)
                    Debug.Print("Pin Map - usage" + i + " : " + PinMap[i]);
                Debug.Print("Pin count size " +size);

                Debug.Print ("------------------------------");
                Debug.Print ("---Pin Usage -------");

                Cpu.PinUsage OnePinUsage;
                for (int i = 0; i < GpioCnt; i++)
                {
                    OnePinUsage = hwProvider.GetPinsUsage((Cpu.Pin)i);
                    if ( OnePinUsage != PinMap[i])
                        Debug.Print ("ERRORR       **** ");
                    Debug.Print("cnt " + i + "usage" + OnePinUsage);
                }
                Debug.Print ("------------------------------");

                Cpu.PinValidResistorMode OnePinResistorMode= hwProvider.GetSupportedResistorModes((Cpu.Pin)0);
                Debug.Print("Reistor Mode " + OnePinResistorMode);

                Debug.Print ("------------------------------");

                Cpu.PinValidInterruptMode OnePinIntMode = hwProvider.GetSupportedInterruptModes((Cpu.Pin)0);
                Debug.Print("Interrupt Mode " + OnePinIntMode);
                Debug.Print ("------------------------------");

                

                // serial                
                Cpu.Pin rxPin;
                Cpu.Pin txPin;
                Cpu.Pin ctsPin;
                Cpu.Pin rtsPin;

                hwProvider.GetSerialPins("COM1", out rxPin, out txPin, out ctsPin, out rtsPin);
                Debug.Print("Serial Port : ");
                Debug.Print("Rx- " + rxPin);
                Debug.Print("Tx- " + txPin);
                Debug.Print("cts- " + ctsPin);
                Debug.Print("Rts- " + rtsPin);

                int SerialNo = hwProvider.GetSerialPortsCount();
                Debug.Print("Total Serial Port : " + SerialNo);

                System.IO.Ports.BaudRate[] StandardBR ;
                hwProvider.GetSupportBaudRates(0, out StandardBR, out size);
                Debug.Print("#Standard Baudrate size" + size);
                for (int i=0; i<size;i++)
                    Debug.Print("Baudrate " + StandardBR[i]);
                Debug.Print ("------------------------------");

                

                bool SupportNonStandardBR = hwProvider.SupportsNonStandardBaudRate(0);
                Debug.Print("Support NonStandard Baudrate " + SupportNonStandardBR);

                uint br = 115200;
                Debug.Print(" support " + br);
                    
                bool result ;
                result = hwProvider.IsSupportedBaudRate(0, ref br);
                Debug.Print(" result " + result + "(" + br + ")");

                uint maxbr, minbr;
                hwProvider.GetBaudRateBoundary(0, out maxbr, out minbr);
                Debug.Print("Com 0 max br" + maxbr + " minBr: " + minbr);
                
                br = maxbr + 1000;
                result = hwProvider.IsSupportedBaudRate(0, ref br);
                Debug.Print(" over max+100 result " + result + "(" + br + ")");

                // SPI
                Cpu.Pin msk;
                Cpu.Pin miso;
                Cpu.Pin mosi;

                hwProvider.GetSpiPins(SPI.SPI_module.SPI1, out msk, out miso, out mosi);
                Debug.Print("SPI Port : ");
                Debug.Print("msk- " + msk);
                Debug.Print("miso- " + miso);
                Debug.Print("mosi- " + mosi);


                int SpiNo = hwProvider.GetSpiPortsCount();
                Debug.Print("Total Spi Port : " + SpiNo);

                // I2C
                Cpu.Pin scl;
                Cpu.Pin sda;

                hwProvider.GetI2CPins(out scl, out sda);
                Debug.Print("I2C Port : ");
                Debug.Print("scl- " + scl);
                Debug.Print("sda- " + sda);

                
                // LCD
                int width, height, orientation, bpp;
                hwProvider.GetLCDMetrics(out width, out height, out bpp, out orientation);
                Debug.Print("width : " + height);
                Debug.Print("Length : " + height);
                Debug.Print("Bit Per Pixel : " + bpp);
                Debug.Print("orientatoin " + orientation);


                //get button -
                Debug.Print("Button Menu pin no " + hwProvider.GetButtonPins(Button.VK_MENU));
                Debug.Print("Button Select pin no " + hwProvider.GetButtonPins(Button.VK_SELECT));
                Debug.Print("Button Back pin no " + hwProvider.GetButtonPins(Button.VK_BACK));                
                Debug.Print("Button Up pin no " + hwProvider.GetButtonPins(Button.VK_UP));
                Debug.Print("Button down pin no " + hwProvider.GetButtonPins(Button.VK_DOWN));
                Debug.Print("Button Left pin no " + hwProvider.GetButtonPins(Button.VK_LEFT));                

                Debug.Print("Button right pin no " + hwProvider.GetButtonPins(Button.VK_RIGHT));  
                Debug.Print("Button home pin no " + hwProvider.GetButtonPins(Button.VK_HOME));                                
                Debug.Print("Button appdef pin no " + hwProvider.GetButtonPins(Button.AppDefined1));                
                Debug.Print("Button VK convert pin no " + hwProvider.GetButtonPins(Button.VK_CONVERT));                



                Thread.Sleep(1000);
               
                }


        }
    }


}
