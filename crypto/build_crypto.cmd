@echo off

REM It would be nice if our build system supported switching between multiple compiler toolchains
REM without dropping out of MSBuild to run a script, allowing everything to be built in one call to
REM MSBuild.

cd %SPOCLIENT%\crypto
IF NOT EXIST private_crypto\dotNetMF.proj GOTO :USAGE

setlocal

set FLAVOR=Release

echo Opening all Crypto binaries for edit:
sd edit lib\...

echo Building Windows crypto libraries:
set COMPILER_TOOL=VS
set COMPILER_TOOL_VERSION=VS9
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building ARC crypto libraries:
pushd %SPOCLIENT%
call setenv_arc
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building ADS1.2 crypto libraries:
pushd %SPOCLIENT%
call setenv_12
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building RVDS3.0 crypto libraries:
pushd %SPOCLIENT%
call setenv_30
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building RVDS3.1 crypto libraries:
pushd %SPOCLIENT%
call setenv_31
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building RVDS4.0 crypto libraries:
pushd %SPOCLIENT%
call setenv_40
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end

echo Building RVDS4.1 crypto libraries:
pushd %SPOCLIENT%
call setenv_41
popd
msbuild build_crypto.proj
IF ERRORLEVEL 1 goto :end


goto :end
:usage

echo To rebuild the crypto libraries, one must have the contents of
echo the SPOT subdirectory of the ENIGMA depot in
echo %%SPOCLIENT%%\crypto\private_crypto

:end
endlocal
