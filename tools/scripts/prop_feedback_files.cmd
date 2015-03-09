@echo off
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION

call %~dp0\init.cmd

cd %CLRROOT%

set FEEDBACKDIR=tools\make\feedback

FOR %%i IN (AUXD iMXS iMXS_NET MOTE2 PCM023) DO (

FOR %%j IN (RVDS3.0 RVDS3.1 RVDS4.0 RVDS4.1) DO (

set BUILDDIR=%BUILD_ROOT_BASE%\arm\%%j

sd edit %FEEDBACKDIR%\%%i_%%j.feedback
sd edit %FEEDBACKDIR%\%%i_%%j_loader.feedback

echo copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinyclr_%%i_FLASH_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback
     copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinyclr_%%i_FLASH_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback

echo copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinybooter_%%i_FLASH_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_Loader.feedback
     copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinybooter_%%i_FLASH_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_loader.feedback

sd add %FEEDBACKDIR%\%%i_%%j.feedback
sd add %FEEDBACKDIR%\%%i_%%j_loader.feedback

)

)

FOR %%i IN (SAM9261_EK) DO (

FOR %%j IN (RVDS3.0 RVDS3.1 RVDS4.0 RVDS4.1) DO (

set BUILDDIR=%BUILD_ROOT_BASE%\arm\%%j

sd edit %FEEDBACKDIR%\%%i_%%j.feedback
sd edit %FEEDBACKDIR%\%%i_%%j_loader.feedback


echo copy /y !BUILDDIR!\RAM\RTM\%%i\bin\tinyclr_%%i_RAM_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback
     copy /y !BUILDDIR!\RAM\RTM\%%i\bin\tinyclr_%%i_RAM_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback

echo copy /y !BUILDDIR!\RAM\RTM\%%i\bin\tinybooter_%%i_RAM_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_loader.feedback
     copy /y !BUILDDIR!\RAM\RTM\%%i\bin\tinybooter_%%i_RAM_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_loader.feedback

sd add %FEEDBACKDIR%\%%i_%%j.feedback
sd add %FEEDBACKDIR%\%%i_%%j_loader.feedback

)

)

FOR %%i IN (iMXS_THUMB iMXS_MNML SAM7X_EK) DO (

FOR %%j IN (RVDS3.0 RVDS3.1 RVDS4.0 RVDS4.1) DO (

set BUILDDIR=%BUILD_ROOT_BASE%\thumb\%%j

sd edit %FEEDBACKDIR%\%%i_%%j.feedback
sd edit %FEEDBACKDIR%\%%i_%%j_loader.feedback


echo copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinyclr_%%i_FLASH_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback
     copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinyclr_%%i_FLASH_RTM_%%j.feedback     %FEEDBACKDIR%\%%i_%%j.feedback

echo copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinybooter_%%i_FLASH_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_Loader.feedback
     copy /y !BUILDDIR!\FLASH\RTM\%%i\bin\tinybooter_%%i_FLASH_RTM_%%j.feedback  %FEEDBACKDIR%\%%i_%%j_loader.feedback

sd add %FEEDBACKDIR%\%%i_%%j.feedback
sd add %FEEDBACKDIR%\%%i_%%j_loader.feedback

)

)

call %SPOCLIENT%\tools\make\feedback\Feedback_FixMultiSections.cmd
