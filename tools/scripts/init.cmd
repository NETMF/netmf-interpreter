@rem
@rem Set some basic vars based on the path of this script
@rem
set TEMPTOOLPATH=%~dp0
@rem Sometimes cmd leaves the trailing backslash on - remove it.
if "%TEMPTOOLPATH:~-1%" == "\" set TEMPTOOLPATH=%TEMPTOOLPATH:~0,-1%

set PREVCD=%CD%
cd %TEMPTOOLPATH%\..\..
set CLRROOT=%CD%
cd %PREVCD%

@rem - CONFIGURATION OF DEBUG vs RELEASE FOR FIRMWARE, TOOLS, AND ASSEMBLIES

if /I "%FLAVOR_DAT%"   == "Debug"   set FLAVOR_DAT=Debug
if /I "%FLAVOR_DAT%"   == "Release" set FLAVOR_DAT=Release
if    "%FLAVOR_DAT%"   == ""        set FLAVOR_DAT=Release

if "%FLAVOR_MEMORY%"   == "" set FLAVOR_MEMORY=Flash

if "%OEM_NAME%"        == "" set OEM_NAME=Microsoft


set FX_35=%WINDIR%\Microsoft.NET\Framework\v3.5
set FX_40=%WINDIR%\Microsoft.NET\Framework\v4.0
set MSBUILD_EXE=%FX_40%\msbuild.exe
set PATH=%PATH%;%FX_40%

if "%COMMON_BUILD_ROOT%"=="" set COMMON_BUILD_ROOT=%CLRROOT%
set BUILD_ROOT_BASE=%COMMON_BUILD_ROOT%\BuildOutput

IF "%COVERAGEBUILDID%" == "" (
	set BUILD_ROOT=%BUILD_ROOT_BASE%\public
) ELSE (
	set BUILD_ROOT=%BUILD_ROOT%\Coverage
)

set BUILD_TREE=%BUILD_ROOT%\%FLAVOR_DAT%
set BUILD_TREE_CLIENT=%BUILD_TREE%\client
set BUILD_TREE_SERVER=%BUILD_ROOT%\%FLAVOR_WIN%\server
set DEVPATH=%BUILD_TREE_SERVER%\dll

set MDP_EXE=%BUILD_TREE_SERVER%\dll\MetadataProcessor.exe
set BHL_EXE=%BUILD_TREE_SERVER%\dll\BuildHelper.exe

IF "%COVERAGEBUILDID%" == "" (
	set BUILD_TEST_ROOT=%BUILD_ROOT%\%FLAVOR_DAT%\Test
) ELSE (
	set BUILD_TEST_ROOT=%BUILD_ROOT%\Coverage\%FLAVOR_DAT%\Test
)
set BUILD_TEST_TREE=%BUILD_TEST_ROOT%
set BUILD_TEST_TREE_CLIENT=%BUILD_TEST_ROOT%\client
set BUILD_TEST_TREE_SERVER=%BUILD_TEST_ROOT%\server


set OEM_PATH=%OEM_ROOT%\%OEM_NAME%
set CLRLIB=%CLRROOT%\Tools\Libraries

set PREVCD=
set TEMPTOOLPATH=

@rem call %~dp0\packpath.cmd

exit /b
