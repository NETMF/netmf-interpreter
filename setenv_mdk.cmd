@echo off

IF NOT ""=="%1" SET MDK_VER=%1
IF ""=="%MDK_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd MDK %MDK_VER% %2 %3 %4 %5

GOTO :EOF

:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_mdk.cmd MDK_VERSION [MDK_TOOL_PATH]
@echo        where MDK_VERSION is the version of the compiler contained in the MDK/RVDS (e.g for MDK 5.14 ARMCC is 5.05)
@echo        where MDK_TOOL_PATH is the path to the ARM directory of the MDK install (i.e. c:\Keil_v5\Arm)
@echo.