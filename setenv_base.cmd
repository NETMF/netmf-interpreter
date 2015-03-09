@echo off

set PORT_BUILD=
set NO_ADS_WRAPPER=
SET COMPILER_TOOL=

if NOT "%1"=="" GOTO :ARGSOK
if NOT "%2"=="" GOTO :ARGSOK

:ERROR
@echo.
@echo Error: Invalid Arguments!
@echo.
@echo Usage: setenv_base COMPILER_TOOL COMPILER_TOOL_VERSION
@echo     where  COMPILER_TOOL         is BLACKFIN, GCC, GCCOP, ADS, RVDS, MDK, SHC, VS
@echo            COMPILER_TOOL_VERSION is the version of the compiler
@echo
@echo     e.g.  setenv.cmd GCC 4.2.1
@echo.
GOTO :EOF


:ARGSOK

SET COMPILER_TOOL=%1
SET COMPILER_TOOL_VERSION_NUM=%2
SET COMPILER_TOOL_VERSION=%1%2
SET ARG3=%~3

SET TFSCONFIG=MFConfig.xml

IF /I NOT "%COMPILER_TOOL%"=="RVDS" IF /I NOT "%COMPILER_TOOL%"=="ADS" set NO_ADS_WRAPPER=1

@ECHO Compiler: %COMPILER_TOOL% %COMPILER_TOOL_VERSION_NUM% %ARG3%

SET CURRENTCD=%CD%

CALL "%~dp0\tools\scripts\init.cmd"

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

SET SPOCLIENT=%CLRROOT%
pushd %SPOCLIENT%\..
SET SPOROOT=%CD%
popd

set NetMfTargetsBaseDir=%SPOCLIENT%\Framework\IDE\Targets\

set _SDROOT_=%SPOROOT:current=%
if "%_SDROOT_:~-1%" == "\" set _SDROOT_=%_SDROOT_:~0,-1%


rem @ make sure we start with a clean path
if "%DOTNETMF_OLD_PATH%"=="" (
goto :save_current_path
) else (
goto :restore_path_from_old
)

:save_current_path
set DOTNETMF_OLD_PATH=%PATH%
goto :after_path_saved_or_restored


:restore_path_from_old
set PATH=%DOTNETMF_OLD_PATH%

:after_path_saved_or_restored

set PATH=%SPOROOT%\tools\NUnit;%SPOROOT%\tools\SDPack;%SPOROOT%\bin;%PATH%
set PATH=%SPOROOT%\tools\x86\perl\bin;%SPOROOT%\tools\x86\bin;%CLRROOT%\tools\bin;%PATH%;%CLRROOT%\tools\scripts
set PATH=%CLRROOT%\BuildOutput\Public\%FLAVOR_WIN%\Test\Server\dll;%PATH%

cd %CURRENTCD%

set CURRENTCD=

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
rem set tool-chains variables 

IF /I NOT "%COMPILER_TOOL%" == "VS" (
    IF NOT "%VS120COMNTOOLS%" == "" ( 
        CALL "%VS120COMNTOOLS%vsvars32.bat"
    ) ELSE (
        IF NOT "%VS110COMNTOOLS%" == "" (
            CALL "%VS110COMNTOOLS%vsvars32.bat"
        ) ELSE (
            IF NOT "%VS100COMNTOOLS%" == "" (
            CALL "%VS100COMNTOOLS%vsvars32.bat"
            ) ELSE (
                IF NOT "%VS90COMNTOOLS%" == "" (
                    CALL "%VS90COMNTOOLS%vsvars32.bat"    
                ) ELSE ( 
                    @ECHO WARNING: Could not find vsvars32.bat.
                    @ECHO WARNING: VISUAL C++ DOES NOT APPEAR TO BE INSTALLED ON THIS MACHINE
                    GOTO :EOF
                )
            )
        )
    )	
)

set TINYCLR_USE_MSBUILD=1   

Title MF %FLAVOR_WIN% (%COMPILER_TOOL% %COMPILER_TOOL_VERSION_NUM%)

IF /I "%COMPILER_TOOL%"=="VS"       GOTO :SET_VS_VARS 

IF /I "%COMPILER_TOOL%"=="ADI"      GOTO :SET_BLACKFIN_VARS
IF /I "%COMPILER_TOOL%"=="GCC"      GOTO :SET_GCC_VARS 
IF /I "%COMPILER_TOOL%"=="GCCOP"    GOTO :SET_GCC_VARS 
IF /I "%COMPILER_TOOL%"=="MDK"      GOTO :SET_MDK_VARS

IF /I "%COMPILER_TOOL%"=="SHC"      GOTO :SET_SHC_VARS 

IF /I "%COMPILER_TOOL%"=="ADS"      GOTO :SET_RVDS_VARS
IF /I "%COMPILER_TOOL%"=="RVDS"     GOTO :SET_RVDS_VARS


IF "%COMPILER_TOOL%"=="" GOTO :ERROR

GOTO :EOF

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_RVDS_VARS
REM dotnetmf team internal setting
set DOTNETMF_COMPILER=%COMPILER_TOOL_VERSION%
SET RVDS_EXT=_v%COMPILER_TOOL_VERSION_NUM:.=_%
SET RVCT_EXT=%COMPILER_TOOL_VERSION_NUM:.=%

IF /I "%COMPILER_TOOL_VERSION_NUM%"=="1.2" (
   SET RVDS_EXT=
   SET DOTNETMF_COMPILER=ADS1.2

   IF NOT "%ARMROOT%" == "%SPOCLIENT%\tools\ads" (
      IF NOT "" == "%ARMHOME%" GOTO :SET_RVDS_V12_VARS
   )
)

setlocal EnableDelayedExpansion 
IF /I NOT "%COMPILER_TOOL_VERSION_NUM%"=="4.1" (
   SET TMPBIN=!RVCT%RVCT_EXT%BIN!
   SET TMPINC=!RVCT%RVCT_EXT%INC!
   SET TMPLIB=!RVCT%RVCT_EXT%LIB!
) ELSE (
   SET TMPBIN=%ARMCC41BIN%
   SET TMPINC=%ARMCC41INC%
   SET TMPLIB=%ARMCC41LIB%
)
endlocal && SET TMPBIN=%TMPBIN%&& SET TMPINC=%TMPINC%&& SET TMPLIB=%TMPLIB%

IF NOT EXIST "%SPOCLIENT%\tools\ads%RVDS_EXT%\BIN\ARMAR.exe" (
IF /I NOT "%ARMROOT%" == "%SPOCLIENT%\tools\ads%RVDS_EXT%" ( 
IF NOT "" == "%TMPBIN%" (
IF EXIST "%TMPBIN%\armar.exe" (

   @echo Using Installed RVDS vars

   SET NO_ADS_WRAPPER=1
   SET ARMHOME=%ARMROOT%
   SET ARMBIN=%TMPBIN%
   SET ARMLIB=%TMPLIB% 
   SET ARMINC=%TMPINC%
   SET ADS_TOOLS=%ARMROOT%
   SET ADS_TOOLS_BIN=%TMPBIN%
   SET RVDS_INSTALLED=TRUE

   SET TMPBIN=
   SET TMPINC=
   SET TMPLIB=
   GOTO :EOF
))))

@echo setting source depot RVDS vars

SET TMPBIN=
SET TMPINC=
SET TMPLIB=

set PATH=%CLRROOT%\tools\ads%RVDS_EXT%\bin;%PATH%
set ARMROOT=%SPOCLIENT%\tools\ads%RVDS_EXT%
set ARMHOME=%ARMROOT%
set ARMLMD_LICENSE_FILE=%ARMROOT%\licenses\license.dat
set ADS_TOOLS=%ARMROOT%
set ARMCONF=%ARMROOT%\BIN
set ARMINC=%ARMROOT%\INCLUDE
set ARMDLL=%ARMROOT%\BIN
set ARMLIB=%ARMROOT%\LIB
set ARMBIN=%ARMROOT%\BIN
set ADS_TOOLS_BIN=%ADS_TOOLS%\BIN

IF NOT "%RVCT_EXT%"=="" (
  set RVCT%RVCT_EXT%BIN=%ARMBIN%
  set RVCT%RVCT_EXT%INC=%ARMINC%
  set RVCT%RVCT_EXT%LIB=%ARMLIB%
)

GOTO :EOF


rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_RVDS_V12_VARS
@echo setting vars for RVDS 1.2 compiler
SET COMPILER_TOOL=RVDS
set DOTNETMF_COMPILER=ADS%COMPILER_TOOL_VERSION_NUM%

set ARMROOT=%ARMHOME%
set ADS_TOOLS_BIN=%ARMDLL%
GOTO :EOF


rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_BLACKFIN_VARS
@ECHO Setting ADI env var and path

set NO_ADS_WRAPPER=1
set DOTNETMF_COMPILER=ADI%COMPILER_TOOL_VERSION_NUM%


IF NOT "%ARG%"     == "" GOTO :DSP_INSTALLED
IF NOT "%ADI_DSP%" == "" GOTO :DSP_INSTALLED

  set ADI_DSP=%CLRROOT%\tools\adi\
  set ANALOGD_LECENSE_FILE=%ADI_DSP%\license.dat

  echo adding   HKLM\Software\Analog Devices\VisualDSP++ %COMPILER_TOOL_VERSION_NUM%\Install Path=%ADI_DSP%
  call reg add "HKLM\Software\Analog Devices" /f 
  call reg add "HKLM\Software\Analog Devices\VisualDSP++ %COMPILER_TOOL_VERSION_NUM%" /f 
  call reg add "HKLM\Software\Analog Devices\VisualDSP++ %COMPILER_TOOL_VERSION_NUM%" /f /v "Install Path" /t REG_SZ /d %ADI_DSP%
  echo adding   HKLM\Software\Analog Devices\VisualDSP++ %COMPILER_TOOL_VERSION_NUM%\License Path=%ADI_DSP%
  call reg add "HKLM\Software\Analog Devices\VisualDSP++ %COMPILER_TOOL_VERSION_NUM%" /f /v "License Path" /t REG_SZ /d %ADI_DSP%

:DSP_INSTALLED

  IF "%ADI_DSP%"=="" set ADI_DSP=%ARG3%

set PATH=%ADI_DSP%;%PATH%

GOTO :EOF


rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_GCC_VARS
@echo setting vars for GCC compiler %COMPILER_TOOL_VERSION%

rem use a default for GCC
IF "%ARG3%"=="" SET ARG3=%SystemDrive%\gnu\gcc
IF NOT EXIST "%ARG3%" GOTO :BAD_GCC_ARG

set ARMROOT=
set ADS_TOOLS_BIN=
set NO_ADS_WRAPPER=1
set GNU_VERSION=%COMPILER_TOOL_VERSION_NUM%
SET COMPILER_TOOL_VERSION_NUM=%COMPILER_TOOL_VERSION_NUM:~0,3%
SET COMPILER_TOOL_VERSION=%COMPILER_TOOL%%COMPILER_TOOL_VERSION_NUM:~0,3%
set DOTNETMF_COMPILER=%COMPILER_TOOL_VERSION%

IF /I "%COMPILER_TOOL%"=="GCC" (
IF EXIST "%ARG3%\lib\gcc\arm-none-eabi\%GNU_VERSION%" (
set ARMINC=%ARG3%\lib\gcc\arm-none-eabi\%GNU_VERSION%\include
set ARMLIB=%ARG3%\lib\gcc\arm-none-eabi\%GNU_VERSION%
set GNU_TOOLS=%ARG3%
set GNU_TOOLS_BIN=%ARG3%\bin
set GNU_TARGET=arm-none-eabi
) ELSE (
@ECHO Could not find %ARG3%\lib\gcc\arm-none-eabi\%GNU_VERSION%
GOTO :BAD_GCC_ARG
))


IF /I "%COMPILER_TOOL%"=="GCCOP" (
IF EXIST "%ARG3%\include\elips_bs" (

set ARMINC=%ARG3%\include\elips_bs
set ARMLIB=%ARG3%\lib
set GNU_TOOLS=%ARG3%
set GNU_TOOLS_BIN=%ARG3%\bin
set GNU_TARGET=arm-elf
set COMPILER_PATH=%ARG3%

) ELSE (
@ECHO Could not find %ARG3%\include\elips_bs
GOTO :BAD_GCC_ARG
))

GOTO :EOF

:BAD_GCC_ARG
@ECHO.
@ECHO Error - Invalid argument (%ARG3%).  Usage: setenv_GCC.cmd GCC_VERSION GCC_TOOL_PATH
@ECHO         Example:  setenv_gcc.cmd 4.2.1 c:\tools\gcc
@ECHO.

GOTO :EOF

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_MDK_VARS
@ECHO Setting MDK env var and path for version %COMPILER_TOOL_VERSION%

rem use a default for MDK
set NO_ADS_WRAPPER=1
set DOTNETMF_COMPILER=%COMPILER_TOOL_VERSION%

IF "%ARG3%"=="" set ARG3=%SystemDrive%\Keil\ARM
IF NOT EXIST "%ARG3%" GOTO :BAD_MDK_ARG

SET MDK_TOOL_PATH=%ARG3%
SET PATH=%MDK_TOOL_PATH%;%PATH%

SET MDK_EXT=%COMPILER_TOOL_VERSION_NUM:~0,1%
SET MDK_SUB_EXT=%COMPILER_TOOL_VERSION_NUM:~2,2%

IF "%MDK_EXT%"=="3" SET MDK_EXT=31
IF "%MDK_EXT%"=="4" (
  IF "%MDK_SUB_EXT%"=="54" (
    SET MDK_EXT=40 
  ) ELSE (
    SET MDK_EXT=31 
  )
)

IF "%MDK_EXT%"=="" (
  @echo Unsupported MDK version %COMPILER_TOOL_VERSION_NUM%
  GOTO :BAD_MDK_ARG
)

SET RVCT%MDK_EXT%BIN=%MDK_TOOL_PATH%\ARM\BIN%MDK_EXT%
SET RVCT31LIB=%MDK_TOOL_PATH%\RV31\LIB
SET RVCT31INC=%MDK_TOOL_PATH%\RV31\INC

GOTO :EOF

:BAD_MDK_ARG
@ECHO.
@ECHO Error - Invalid argument.  Could not find MDK path %ARG3%.
@ECHO.

GOTO :EOF

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_VS_VARS
set VSSDK11INSTALLDIR=%SPOROOT%\tools\x86\MicrosoftSDKs\VSSDK\vs11\
if NOT EXIST "%VSSDK11INSTALLDIR%" set VSSDK11INSTALLDIR=%VS110COMNTOOLS%VSSDK\

set VSSDK12INSTALLDIR=%SPOROOT%\tools\x86\MicrosoftSDKs\VSSDK\vs12\
if NOT EXIST "%VSSDK12INSTALLDIR%" set VSSDK12INSTALLDIR=%VS120COMNTOOLS%VSSDK\

set VSSDK14INSTALLDIR=%SPOROOT%\tools\x86\MicrosoftSDKs\VSSDK\vs14\
if NOT EXIST "%VSSDK14INSTALLDIR%" set VSSDK14INSTALLDIR=%VS140COMNTOOLS%VSSDK\

set NO_ADS_WRAPPER=1
set DOTNETMF_COMPILER=%COMPILER_TOOL_VERSION%

IF "%COMPILER_TOOL_VERSION_NUM%"=="9" (
  IF "" == "%VS90COMNTOOLS%" GOTO BAD_VS_ARG
  CALL "%VS90COMNTOOLS%vsvars32.bat"
  GOTO :EOF
)

IF "%COMPILER_TOOL_VERSION_NUM%"=="10" (
  IF "" == "%VS100COMNTOOLS%" GOTO BAD_VS_ARG
  CALL "%VS100COMNTOOLS%vsvars32.bat"
  GOTO :EOF
)

IF "%COMPILER_TOOL_VERSION_NUM%"=="11" (
  IF "" == "%VS110COMNTOOLS%" GOTO BAD_VS_ARG
  CALL "%VS110COMNTOOLS%vsvars32.bat"
  GOTO :EOF
)

IF "%COMPILER_TOOL_VERSION_NUM%"=="12" (
  IF "" == "%VS120COMNTOOLS%" GOTO BAD_VS_ARG
  CALL "%VS120COMNTOOLS%vsvars32.bat"
  GOTO :EOF
)

:BAD_VS_ARG
@ECHO.
@ECHO Error - Invalid argument.
@ECHO Could not find VSVARS32.bat for VS%COMPILER_TOOL_VERSION_NUM%
@ECHO.

GOTO :EOF

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
:SET_SHC_VARS

SET COMPILER_TOOL=SHC
set NO_ADS_WRAPPER=1
set DOTNETMF_COMPILER=%COMPILER_TOOL_VERSION%
SET SHC_VER_PATH=%COMPILER_TOOL_VERSION_NUM:.=_%

IF "%ARG3%"=="" (
  SET SHC_TOOL_PATH=%SPOCLIENT%\tools\SH\%SHC_VER_PATH%
) ELSE (
  SET SHC_TOOL_PATH=%ARG3:"=%
)

IF NOT EXIST "%SHC_TOOL_PATH%\BIN\SHCPP.exe" SET SHC_TOOL_PATH=%SHC_TOOL_ROOT%%SHC_VER_PATH%

IF NOT EXIST "%SHC_TOOL_PATH%\BIN\SHCPP.exe" GOTO :BAD_SHC_ARG

set PATH=%SHC_TOOL_PATH%\bin;%PATH%
set SHC_LIB=%SHC_TOOL_PATH%\bin
set SHC_INC=%SHC_TOOL_PATH%\include
set SHC_TMP=%SHC_TOOL_PATH%\CTemp
set SHC_TOOLS_BIN=%SHC_TOOL_PATH%\bin

GOTO :EOF

:BAD_SHC_ARG
SET SHC_TOOL_PATH=

@ECHO.
@ECHO Error - Invalid argument.  Usage: setenv.cmd SHC_TOOL_PATH
@ECHO         Example:  setenv.cmd c:\sh\9_2_0
@ECHO.

GOTO :EOF

rem @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
