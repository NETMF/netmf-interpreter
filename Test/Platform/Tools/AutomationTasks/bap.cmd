@echo off
setlocal

if /i not "%FLAVOR_WIN%"=="Debug" (
  echo Set environment for debug build
  goto :END
)

set DEPLOY_AUTO_BUILD=false
set BUILD_SUPPORT_LIBRARIES=false

:GETARGS

if "%1"=="" goto :START


shift
goto :GETARGS

:START



:BUILDTASKS


msbuild AutomationTasks.csproj 

sd edit %SPOCLIENT%\tools\build\Microsoft.SPOT.Automation.Build.Branch.*
copy /y %BUILD_TREE_SERVER%\dll\Microsoft.SPOT.Automation.Build.Branch.* %SPOCLIENT%\tools\build


:END
endlocal