@ECHO OFF

IF "%1" == "" goto :BAD_ARGS
IF NOT EXIST "%1" goto :BAD_ARGS

@echo setting source depot MetaWare vars
set PATH=%1\tools\mtwr_8_0\arc\bin;%PATH%

set ARMROOT=
set ADS_TOOLS_BIN=

SET MTWR_ROOT=%1\tools\mtwr_8_0
set MTWR_LMD_LICENSE_FILE=%MTWR_ROOT%\ARC\Licenses\license.dat
set MTWR_INC=%MTWR_ROOT%\INCLUDE
set MTWR_LIB=%MTWR_ROOT%\ARC\LIB\%ARC_LIB%
set MTWR_BIN=%MTWR_ROOT%\ARC\BIN
set LM_LICENSE_FILE=%MTWR_ROOT%\arc\Licenses\license.dat


goto :EOF

:BAD_ARGS
@ECHO.
@ECHO Invalid arguments!  Usage: InternalSetArcVars.cmd SPOCLIENT
@ECHO     Where SPOCLIENT is the //depot/current/client_vx_x path
@ECHO.