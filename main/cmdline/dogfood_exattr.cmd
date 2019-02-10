@echo off
pushd %cd%
setlocal
@set ExcludeAdminOnly=--where:cat!=AdminOnly
@if "%appveyor%" == "True" set ExcludeAdminOnly=
cd %~dp0
OpenCover.Console.exe -register:user -target:..\..\..\main\packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe -targetargs:"OpenCover.Test.dll --x86 %ExcludeAdminOnly%" -excludebyattribute:*.ExcludeFromCoverageAttribute -filter:"+[Open*]* -[OpenCover.T*]* -{nunit-console*}[*]* -{pdb*}[*]*" -output:opencovertests.xml -mergebyhash
endlocal
popd
