@echo off
SETLOCAL

IF /I "%1"=="" (
    goto Usage
)
IF "%2"=="" (
    goto Usage
)

PUSHD %SPOCLIENT%\tools\bin 
MetaDataProcessor.exe -sign_file %1 tinybooter_private_key.bin %2
MetaDataProcessor.exe -verify_signature %1 tinybooter_public_key.bin %2
POPD

:Usage

echo Usage:
echo sign_file ^<file to sign^> ^<signature file^>
echo example: 
echo sign_file %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.bin\ER_CONFIG  %SPOCLIENT%_BUILD\arm\FLASH\release\AUXD\bin\tinyclr.hex\ER_CONFIG.sig
