setlocal
call setenv_gcc 5.4.1 "E:\GNU_Tools_ARM_Embedded\5_4_2016q3"
cd Solutions\MCBSTM32F400
msbuild dotnetmf.proj /t:build /p:flavor=release;memory=flash /filelogger /flp:ErrorsOnly  /clp:verbosity=minimal /maxcpucount /v:m 
@REM must rename the image file to include the HEX extension
cd..
cd..
cd BuildOutput\THUMB2FP\GCC5.4\le\FLASH\release\MCBSTM32F400\bin\tinyclr.hex
rename ER_FLASH ER_FLASH.hex
endlocal&&exit /B %ERRORLEVEL%
