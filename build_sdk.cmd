@ECHO OFF

@REM The automated build process can, and should, set the following environment variables prior to calling this command script
@REM SignBuild=true           ; Submits the resulting binaries for signing, this will fail if the user does not have CODESIGN permissions
@REM AUTOMATED_BUILD=true     ; Indicates to the build that this is an automated build process, there are a few references to this and we hope to eliminate them in the future

if "%1" == "/?" goto :ShowUsage
if "%1" == "-?" goto :ShowUsage
if /i "%1" == "/h" goto :ShowUsage
if /i "%1" == "-h" goto :ShowUsage

if /i "%VSSDK140Install%"=="" goto :MissingVSSDK
if NOT EXIST "%VSSDK140Install%" goto :MissingVSSDK

SET BUILD_VERSION=%1
if "%BUILD_VERSION%"=="" set BUILD_VERSION=0
SET BUILD_SHARE=%2
if "%BUILD_SHARE%"=="" set BUILD_SHARE=%SPOCLIENT%
SET BUILD_BRANCH=%3
SET RELEASENAME=%4
if "%RELEASENAME%"=="" set RELEASENAME="(%USERNAME%)"
SET WixMsiBuildNumberOverride=%5

SET PRODVER_MAJOR=4
SET PRODVER_MINOR=4
IF "%WixMsiBuildNumberOverride%"=="" set WixMsiBuildNumberOverride=%BUILD_VERSION%

set COMMON_BUILD_ROOT=%BUILD_SHARE%
if NOT "%BUILD_BRANCH%"=="" set COMMON_BUILD_ROOT=%COMMON_BUILD_ROOT%\%BUILD_BRANCH%

call setenv_vs.cmd 14

SET PORT_BUILD=

ECHO Building PreSDK ...
call Msbuild sdk.dirproj /nr:false /t:Build /p:BuildNumber=%BUILD_VERSION% /p:FLAVOR=RTM  /clp:verbosity=minimal /flp:verbosity=detailed;LogFile=sdkpre.log

ECHO Building SDK ...
call Msbuild setup\ProductSDK\Product.wixproj /m /t:Build /p:BuildNumber=%BUILD_VERSION% /p:FLAVOR=RTM /clp:verbosity=minimal /flp:verbosity=detailed;LogFile=sdk.log

ECHO Building VSIX packages ...
call Msbuild setup\ProductSDK\VsixPackages.dirproj /t:Build /p:BuildNumber=%BUILD_VERSION% /p:FLAVOR=RTM /clp:verbosity=minimal /flp:verbosity=detailed;LogFile=vsixpkg.log

SET PORT_BUILD=

GOTO :EOF

:ShowUsage
@ECHO usage:
@ECHO     build_sdk.cmd [BUILD_NUMBER] [BUILD_SHARE] [BUILD_BRANCH] [RELEASE_NAME]
@ECHO   or
@ECHO     build_sdk.cmd
@ECHO where:
@ECHO     BUILD_NUMBER = Build portion of the version quad [Default = 0 ]
@ECHO     BUILD_SHARE = Root of the build output [ Default = SPOCLIENT ] 
@ECHO     BUILD_BRANCH = Branch used for the build [ Default = client_vNext ]
@ECHO     RELEASE_NAME = Name for the release [ Default = "(%%USERNAME%%)"]
@echo example:
@ECHO     build_sdk.cmd 1234 \\NETMFBLD02\Builds\69423 client_vNext "(RC1)"
goto :EOF

:MissingVSSDK
@ECHO ERROR: Visual Studio 2015 SDK (VSSDK) was not detected, this SDK is required to build the .NET Micro Framework SDK source code
goto :EOF