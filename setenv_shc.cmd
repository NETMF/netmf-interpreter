@echo off

IF NOT ""=="%1" SET SHC_VER=%1
IF ""=="%SHC_VER%" SET SHC_VER=9.2.0

%~dp0\setenv_base.cmd SHC %SHC_VER% %2 %3 %4 %5

GOTO :EOF

:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_shc.cmd SHC_VERSION
@echo        where SHC_VERSION is (9.2.0, 9.4.0, ...)
@echo.