@echo off

REM 
REM This script is used to extract a list of libraries under a .\tmp directory
REM

set PREVCD=%CD%

for %%i IN (%*) do (
  IF "%%i"=="%1" (
    IF NOT EXIST %1\tmp mkdir %1\tmp
    cd %1\tmp
  ) ELSE (
    IF "%GNU_TARGET%"=="arm-elf" (
      @echo "%GNU_TOOLS_BIN%\ar -x ..\%%i"
      "%GNU_TOOLS_BIN%\ar" -x ..\%%i
    ) ELSE (
      IF EXIST "%GNU_TOOLS_BIN%\%GNU_TARGET%-ar.exe" (
        @echo "%GNU_TOOLS_BIN%\%GNU_TARGET%-ar -x ..\%%i"
        "%GNU_TOOLS_BIN%\%GNU_TARGET%-ar" -x ..\%%i
      ) ELSE (
        IF EXIST "%GNU_TOOLS_BIN%\..\%GNU_TARGET%\bin\ar.exe" (
          @echo "%GNU_TOOLS_BIN%\..\%GNU_TARGET%\bin\ar -x ..\%%i"
          "%GNU_TOOLS_BIN%\..\%GNU_TARGET%\bin\ar" -x ..\%%i
        ) ELSE (
          @echo ERROR: Cannot find GCC archiver tool!
          GOTO :DONE
        )
      )
    )
  )
)

:DONE

cd %PREVCD%
set PREVCD=