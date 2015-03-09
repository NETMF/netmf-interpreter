@echo off
SETLOCAL

call %~dp0\init.cmd

if not exist %BUILD_TREE_CLIENT%\Stubs md %BUILD_TREE_CLIENT%\Stubs

@rem ################################################################################

echo Generating String Table...
%PRG_MMP% -verbose -cfg %CLRLIB%\opt_stringtable.cfg & if ERRORLEVEL 1 exit /b 10
cd ..
