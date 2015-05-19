@echo off

IF NOT "" == "%1" SET VS_VER=%1
IF "" == "%VS_VER%" SET VS_VER=14

%~dp0\setenv_base.cmd VS %VS_VER% %2 %3 %4 %5

GOTO :EOF


:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_vs.cmd VS_VERSION
@echo        where VS_VERSION is (9, 10, 11, ...)
@echo.

