@ECHO OFF

IF "%1"=="" GOTO :BADARG
IF "%2"=="" GOTO :BADARG
IF "%3"=="" GOTO :BADARG

SET BUILD_VERSION=%1
SET BUILD_SHARE=%2
SET BUILD_BRANCH=%3
SET RELEASENAME=%4

SET PRODVER_MAJOR=4
SET PRODVER_MINOR=3

set COMMON_BUILD_ROOT=%BUILD_SHARE%\%BUILD_BRANCH%

call setenv_vs.cmd 11

SET PORT_BUILD=

ECHO Building PreSDK ...
call Msbuild sdk.dirproj /m:%NUMBER_OF_PROCESSORS% /t:Build /p:BuildNumber=%BUILD_VERSION% /p:SignBuild=true /p:FLAVOR=RTM /p:AUTOMATED_BUILD=true > sdkpre.log

ECHO Building SDK ...
call Msbuild /m:%NUMBER_OF_PROCESSORS% setup\ProductSDK\Product.wixproj /t:Build /p:BuildNumber=%BUILD_VERSION% /p:SignBuild=true /p:FLAVOR=RTM /p:AUTOMATED_BUILD=true  > sdk.log

ECHO Generating port ...
CALL generate_port %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1 TEMPLATE Debug;Release;Instrumented;RTM copy_only build_automation zip BuildNumber=%BUILD_VERSION% > generate_port.log

ECHO Partitioning PK ...
CALL PartitionPK.exe %SPOCLIENT%\Setup\PKProductNoLibs\PKProductNoLibs.wxs MicroFrameworkPK %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;ProdReleaseName=%RELEASENAME%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" Crypto\LIB;DeviceCode\PAL\rtip\LIB\RVDS3.1 ARM=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH% > partitionpk.log

@ECHO PK-CRYPTO
CALL PartitionPK.exe PKProductCrypto\PKProductCrypto.wxs "MicroFrameworkPK (Crypto Pack)" %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdReleaseName=%RELEASENAME%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" NONE CRYPTO=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH%\Crypto\LIB > pkcrypto.log

@ECHO PK-NET-ARM
CALL PartitionPK.exe PKProductNetARM\PKProductNetARM.wxs "MicroFrameworkPK (ARM Network Pack)" %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdReleaseName=%RELEASENAME%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" THUMB;THUMB2 ARMNET=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH%\DeviceCode\PAL\rtip\LIB\RVDS3.1 > pknetarm.log

@ECHO PK-NET-THUMB
CALL PartitionPK.exe PKProductNetThumb\PKProductNetThumb.wxs "MicroFrameworkPK (THUMB Network Pack)" %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdReleaseName=%RELEASENAME%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" ARM;THUMB2 THUMBNET=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH%\DeviceCode\PAL\rtip\LIB\RVDS3.1 > pknetthumb.log

@ECHO PK-NET-THUMB2
CALL PartitionPK.exe PKProductNetThumb2\PKProductNetThumb2.wxs "MicroFrameworkPK (THUMB2 Network Pack)" %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdReleaseName=%RELEASENAME%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" ARM;THUMB THUMB2NET=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH%\DeviceCode\PAL\rtip\LIB\RVDS3.1 > pknetthumb2.log

@ECHO PK-NET-SH2
CALL PartitionPK.exe PKProductNetSH2\PKProductNetSH2.wxs "MicroFrameworkPK (SH2 Network Pack)" %BUILD_SHARE%\%BUILD_BRANCH%_PortingKits BuildFlavor=;BuildTreeServer=%SPOCLIENT%\BuildOutput\public\Release\server;BuildTreeClient=%SPOCLIENT%\BuildOutput\public\Release\client;BuildTreeClientTest=%SPOCLIENT%\BuildOutput\public\Release\test\client;BuildTreeServerTest=%SPOCLIENT%\BuildOutput\public\Release\test\server;WixSpoClient=%SPOCLIENT%;ProdVerMajor=%PRODVER_MAJOR%;ProdVerMinor=%PRODVER_MINOR%;ProdVerBuild=%BUILD_VERSION%;ProdReleaseName=%RELEASENAME%;ProdVer=%PRODVER_MAJOR%.%PRODVER_MINOR%.%BUILD_VERSION%.0;ProdVerMajorMinor=%PRODVER_MAJOR%.%PRODVER_MINOR%;MSBuildProjectDirectory="%SPOCLIENT%";MSBuildProjectFile=%BUILD_BRANCH%.branchproj;MSBuildProjectExtension=.branchproj;MSBuildProjectFullPath="%SPOCLIENT%\%BUILD_BRANCH%.branchproj";MSBuildProjectName=%BUILD_BRANCH%;MSBuildBinPath="%FX_40%";MSBuildProjectDefaultTargets=Build;MSBuildExtensionsPath="%ProgramFiles%\MSBuild";DesktopTargetFrameworkVersion=2.0.50727;VisualStudioIntegrationDir="%VSSDK110Install%\VisualStudioIntegration" NONE SH2NET=%BUILD_SHARE%\%BUILD_BRANCH%_PortingKits\Template3.1\%BUILD_BRANCH%\DeviceCode\PAL\rtip\LIB\HEW9.2  > pknetSH.log

SET PORT_BUILD=

GOTO :EOF

:BADARG
@ECHO Bad arguments! usage: build_sdk_and_port.cmd [BUILD_NUMBER] [BUILD_SHARE] [BUILD_BRANCH] [RELEASE_NAME]
@ECHO e.g. build_sdk_and_port.cmd 2821 \\NETMFBLD02\Builds\69423 client_v4_3 "(RC1)"

:EOF
