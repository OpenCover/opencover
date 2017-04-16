@echo off
pushd %cd%
setlocal
@set ExcludeAdminOnly = --where:cat!=AdminOnly
@if "%appveyor%" == "True" set ExcludeAdminOnly=
cd %~dp0
OpenCover.Console.exe -register:user -safemode:on -target:..\..\..\main\packages\NUnit.ConsoleRunner.3.5.0\tools\nunit3-console.exe -targetargs:"OpenCover.Test.dll OpenCover.Integration.Test.dll --x86 %ExcludeAdminOnly%" -filter:"+[Open*]* -[OpenCover.T*]* -<nunit3-console*>[*]* -<pdb*>[*]*" -output:opencovertests.xml -communicationtimeout:9999
endlocal
popd
