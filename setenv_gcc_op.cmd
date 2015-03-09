@echo off

REM # provide the gcc open plug's compiler tool dir, as the original tool from OP's  just dump in a arm-elf
REM # have to rearrange the tools a bit
REM # arm-elf\bin for all the .exe
REM # arm-elf\include for all the .h
REM # arm-elf\lib for all the libs ( .a)

IF NOT ""=="%1" SET GCCOP_VER=%1
IF ""=="%GCCOP_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd GCCOP %GCCOP_VER% %2 %3 %4 %5

GOTO :EOF


:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_gccop.cmd GCCOP_VERSION [GCCOP_TOOL_PATH]
@echo        where GCCOP_VERSION is (4.2, ...)
@echo.
