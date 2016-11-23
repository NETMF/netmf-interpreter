@echo off

IF NOT ""=="%1" SET DS5_VER=%1
IF ""=="%DS5_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd DS5 %DS5_VER% %2 %3 %4 %5

GOTO :EOF

:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_ds.cmd DS5_VERSION [DS5_TOOL_PATH]
@echo        where DS5_VERSION is the version of the compiler contained in the MDK/RVDS/DS5 (e.g for MDK 5.14 ARMCC is 5.05)
@echo        where DS5_TOOL_PATH is the path to the ARM directory of the DS install (i.e. c:\Keil_v5\Arm)
@echo.