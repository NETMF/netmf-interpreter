@echo off

IF NOT ""=="%1" SET MDK_VER=%1
IF ""=="%MDK_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd MDK %MDK_VER% %2 %3 %4 %5

GOTO :EOF

:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_mdk.cmd MDK_VERSION [MDK_TOOL_PATH]
@echo        where MDK_VERSION is (3.80a, 4.12, 4.13, ...)
@echo        where MDK_TOOL_PATH is the path to the ARM directory of the MDK install (c:\Keil\Arm)
@echo.