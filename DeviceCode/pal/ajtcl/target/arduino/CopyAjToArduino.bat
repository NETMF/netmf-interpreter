SETLOCAL

SET SRC=c:\cygwin\home\%USERNAME%\repos\alljoyn-tc\ajtcl
SET DST=c:\arduino\arduino-1.5.2\hardware\arduino\sam\libraries\AllJoyn

rmdir /S /Q %DST%
xcopy /Y /D /S /I %SRC%\build\arduino_due\libraries\AllJoyn %DST%
