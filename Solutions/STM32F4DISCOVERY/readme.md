### Recipe to configure a NETMF solution ###

Please note the following (**and this is _EXTREMELY_ important**): 
- you must keep the solution name consistent throughout the find/replace/renaming that follows. Failing to do so will result in compilation errors and possibly in bugs that might be hard to track specially if they are using other solutions that already exist in the repository tree.
- In all proj files and some settings file there are GUIDs with tags ```<PlatformGuid>``` and ```<ProjectGuid>```. You have to replace them with new ones.   


##### Pre-requisites #####

In order to start configuring a new solution you need to check if the HAL/PAL of the MCU you are targeting are already implemented.
The current NETMF implementation is based in CMSIS. You may want to look into the following folders and search for the appropriate MCU vendor and series/familly names:
- \CMSIS\Device\...
- \DeviceCode\Targets\Native\...

For example, to implement a solution targeting ST's STM32F4-DISCOVERY board, you should find there:
- \CMSIS\Device\\**ST**\\**STM32F4xx** 
- \DeviceCode\Targets\Native\\**STM32F4xx**  

The CMSIS folder has the header files for the STM32F4 series.
The Tartgets\Native folder has the header and code that implements the HAL for the STM32F4 series.

If these don't exist then you must start by adding support for the appropriate vendor and series/familly CMSIS and HAL/PAL.

##### Step-by-step #####

1. Start by copying an existing solution folder and rename it to something appropriate. For obvious reasons it's recommended that you choose one that is similar to the MCU/board that you are targeting. 

2.  Edit the **dotnetmf.proj** file (located in the solution home folder).
  1. Find/replace the original solution name in the first ```<PropertyGroup>``` block.
  2. Near the bottom finf the group ```<Target Name="BeforeBuild">``` and at ```<exec command="powershell.exe...```) check if the script is the appropriate on for the vendor and series/familly you are targeting.  
  (in the example the sript being called is for the STM32 familly that supports all the current ST STM32 series)
  
3. Rename the existing **_SOLUTIONNAME_** **.settings** to the new solution name.
  1. Assign a new ```<PlatformGuid>```.
  2. Find/replace the original solution name throughout the file.
  3. Find the ```<PropertyGroup>``` block that includes the device ID and replace it with the appropriate names.
  4. Make sure the last ```<Import Project="$(SPOCLIENT)\\devicecode\\Targets``` is pointing to the correct HAL for the device you are targeting.
  
4. Edit the **xxxxxxxxx_hal_conf.h** file.
  1. Note that you may have to start a new 'hal_conf' file from the appropriate template if you are targeting an MCU that belong to a different vendor/series.  
  2. Adjust the following values according to the hardware of your target solution
    * external oscilator clock (HSE_VALUE)
    * if used, low speed oscilator clock (LSI_VALUE) 
    * if used, external low speed oscilator clock (LSE_VALUE)
    * voltage powering the MCU (VDD) 
    * the interrupt priority for the system tick (TICK_INT_PRIORITY)
    * if you are using a CMSIS compliant RTOS (USE_RTOS)

5. Edit the **platform_selector.h** file.
  1. Find/replace the original solution name with the new one.
  (TBD)
  
6. Edit the **features.settings** file in the TinyBooter folder.
  1. Set to TRUE/FALSE the features that you want to enable for the TinyBooter configuration of your solution. 
  _(Keep in mind that the the TinyBooter flash is very limited and that the TinyBooter should only provide the absolute minimum required funcionality to boot the hardware)_

7. If you have any specific requirements in want concerns any specfic framework features edit the **TinyBooter.proj** file in the TinyBooter folder and change it accordingly, otherwise you shouldn't need touch this file. 
  
8. Edit the **features.settings** file in the TinyCLR folder.
  1. Set to TRUE/FALSE the features that you want to enable for the TinyCLR configuration of your solution.

9. If you have any specific requirements in want concerns any specfic framework features edit the **TinyCLR.proj** file in the TinyCLR folder and change it accordingly, otherwise you shouldn't need touch this file.
 
