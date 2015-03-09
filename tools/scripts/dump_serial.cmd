@echo off
SETLOCAL
SETLOCAL ENABLEEXTENSIONS
SETLOCAL ENABLEDELAYEDEXPANSION

@rem ################################################################################

if "%1" == "/?" goto usage
if "%1" == "?" goto usage

SerialDump.exe %*

exit/b

:usage
Echo Usage: %SCRIPT% ^[options ...^] ^[^<serial port^>^] ^[^<serial settings^>^]
Echo.
Echo Options: -dump ^<filename^> 	Dump output also to a file.
Echo          ^<serial port^>		Serial port to use		     (Default: COM1  )
Echo          ^<serial settings^>	Settings for the serial port (Default: 115200)

exit/b
