@echo off

set COMMON_TEST=%CD%

pushd ..\..\common\test
call setenv.cmd

popd
