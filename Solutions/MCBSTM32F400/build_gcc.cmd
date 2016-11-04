echo off
echo Set Environnement variables for GCC
call E:\GitHub\netmf-interpreter\setenv_gcc 5.4.1 E:\GNU_Tools_ARM_Embedded\5_4_2016q2
call msbuild /t:rebuild /p:flavor=release;memory=flash /filelogger /flp:verbosity=detailed /clp:verbosity=minimal /maxcpucount  
del E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.bin\*.bin
ren E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.bin\ER_CONFIG ER_CONFIG.bin
ren E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.bin\ER_FLASH ER_FLASH.bin
del E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.hex\*.hex
ren E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.hex\ER_CONFIG ER_CONFIG.hex
ren E:\GitHub\netmf-interpreter\BuildOutput\THUMB2FP\GCC4.9\le\FLASH\release\STM32F4DISCOVERY\bin\tinyclr.hex\ER_FLASH ER_FLASH.hex
pause