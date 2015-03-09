@echo off

IF NOT ""=="%1" SET RVDS_VER=%1
IF ""=="%RVDS_VER%" GOTO :ARG_ERROR

%~dp0\setenv_base.cmd RVDS %RVDS_VER% %2 %3 %4 %5

GOTO :EOF


:ARG_ERROR
@echo.
@echo ERROR: Invalid version argument.
@echo USAGE: setenv_rvds.cmd RVDS_VERSION
@echo        where RVDS_VERSION is (1.2, 3.0, 3.1, 4.0, ...)
@echo.