@echo off

if not defined SPOCLIENT (
   echo Need to run setenv.cmd first
   goto :END
)

setlocal

call "%VS110COMNTOOLS%\vsvars32.bat"

set _installDirProject="%USERPROFILE%\AppData\Roaming\Microsoft\VisualStudio\11.0\ProjectTemplates\CSharp\Micro Framework Test"
set _installDirProject1033="%USERPROFILE%\AppData\Roaming\Microsoft\VisualStudio\11.0\ProjectTemplates\CSharp\Micro Framework Test\1033"
set _installDirItem="%USERPROFILE%\AppData\Roaming\Microsoft\VisualStudio\11.0\ItemTemplates\CSharp\1033"
set _devEnvExe="%VS110COMNTOOLS%..\IDE\devenv.exe"

@echo off
if exist %_installDirProject1033% rd /s /q %_installDirProject1033%
if exist %_installDirProject% rd /s /q %_installDirProject%

mkdir %_installDirProject%
mkdir %_installDirProject1033%

if exist %_installDirProject1033%"\Micro Framework CSharp Test.zip" del %_installDirProject1033%"\Micro Framework CSharp Test.zip"
if exist %_installDirProject1033%"\Micro Framework CSharp Deskptop Test.zip" del %_installDirProject1033%"\Micro Framework CSharp Desktop Test.zip"
if exist %_installDirProject%"\MicroFrameworkTest.vstdir" del %_installDirProject%"\MicroFrameworkTest.vstdir"
if exist %_installDirItem%"\Micro Framework Test Case.zip" del %_installDirItem%"\Micro Framework Test Case.zip"

echo "Copying the template files..."
copy "%SPOCLIENT%\Test\Platform\Tools\TestTemplates\DevEnvironment\Micro Framework CSharp Test.zip" %_installDirProject1033%
copy "%SPOCLIENT%\Test\Platform\Tools\TestTemplates\DevEnvironment\Micro Framework CSharp Desktop Test.zip" %_installDirProject1033%
copy "%SPOCLIENT%\Test\Platform\Tools\TestTemplates\DevEnvironment\Micro Framework Test Case.zip" %_installDirItem%
copy "%SPOCLIENT%\Test\Platform\Tools\TestTemplates\MicroFrameworkTest.vstdir" %_installDirProject%

echo "Adding the test template to visual studio..."
%_devEnvExe% /setup /rootsuffix Exp /ranu
echo "Done..."

echo "Updating registry entries..."
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v3_0"     /t REG_SZ /f /d "%SPOROOT%\client_v3_0\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v3_0_dev" /t REG_SZ /f /d "%SPOROOT%\client_v3_0_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_0"     /t REG_SZ /f /d "%SPOROOT%\client_v4_0\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_0"     /t REG_SZ /f /d "%SPOROOT%\client_v4_0\tools\Targets\Microsoft.SPOT.Test.CSharp.Host.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_0_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_0_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_0_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_0_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Host.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_2_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_2_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_2_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_2_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Host.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_3_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_3_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Targets"
reg ADD "HKLM\Software\Microsoft\VisualStudio\11.0\Configuration\MSBuild\SafeImports" /v ".NET Micro Framework CSharp Test Target client_v4_3_dev" /t REG_SZ /f /d "%SPOROOT%\client_v4_3_dev\tools\Targets\Microsoft.SPOT.Test.CSharp.Host.Targets"
echo "Done..."

:End
endlocal
