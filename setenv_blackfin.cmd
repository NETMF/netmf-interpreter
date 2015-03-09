@echo off

IF NOT ""=="%1" SET BF_VER=%1
IF ""=="%BF_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd ADI %BF_VER% %2 %3 %4 %5

GOTO :EOF


:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_blackfin.cmd BLACKFIN_VERSION
@echo        where BLACKFIN_VERSION is (5.0, ...)
@echo.

