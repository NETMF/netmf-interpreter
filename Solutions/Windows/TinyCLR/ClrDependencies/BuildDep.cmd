Echo running command %2 for FLAVOR=%3 with SPOCLIENT=%1
pushd %1
call setenv_vs 14
cd solutions\Windows
msbuild /p:FLAVOR=%3 /t:%2 /flp:verbosity=diagnostic;LogFile=BuildDep.log /clp:verbosity=minimal
popd
