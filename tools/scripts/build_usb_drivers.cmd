@echo off

REM    ***********************************
REM     MODIFIED to support WDK April 2008
REM    ***********************************
REM
REM    Purpose: Builds kernel-mode USB drivers for Windows XP(32-bit)
REM                                                Windows Vista/Longhorn (32 and 64 bit)
REM
REM    USAGE: build_usb_drivers.cmd  WDK_INSTALLDIR  [release|debug]
REM           Omitting release or debug flag means both flavors are built.
REM

setlocal ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

set FLAVOR_WIN="BOTH"

IF "%1"==""           goto :USAGE
IF NOT EXIST "%1"     goto :USAGE

set DDK_BASEDIR=%1


IF /i "%2"=="release"  (
    set FLAVOR_WIN="release"
    goto :BLD )

IF /i "%2"=="debug"    (
    set FLAVOR_WIN="debug"
    goto :BLD )


:BLD


if /I %FLAVOR_WIN% EQU "debug" goto :DEBUG


REM    ****    BUILD USB FREE VERSIONS     ****


SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre wxp 
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre wxp
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL

SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre wlh
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre wlh
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL

SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre x64 wlh  
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% fre x64 wlh  
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL



:DEBUG


REM    ****    BUILD USB CHECK VERSIONS     ****


if /I %FLAVOR_WIN% EQU "release" goto :END


SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk wxp 
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk wxp
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL

SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk wlh
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk wlh
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL

SETLOCAL
echo %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk x64 wlh  
call %DDK_BASEDIR%\bin\setenv.bat %DDK_BASEDIR% chk x64 wlh  
PUSHD %SPOCLIENT%\USB_DRIVERS\MFUSB_PortingKitSample
BUILD
POPD
ENDLOCAL



goto :end


:USAGE

ECHO.
ECHO ***** Error: Invalid Arguments!
ECHO.
ECHO       USAGE:  BUILD_USB  WDK_INSTALLDIR ^[release^|debug^]
ECHO.
ECHO       if release or debug flag is omitted, then both flavors are built. 
ECHO.
ECHO       See readme.txt for setup and additional information.




:end
