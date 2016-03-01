@echo off
setlocal
call setenv_%1 %2
cd Solutions\%3
msbuild /flp:verbosity=detailed /clp:verbosity=minimal
endlocal