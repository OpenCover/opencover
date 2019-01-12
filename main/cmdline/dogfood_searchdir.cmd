@echo off
pushd %cd%
setlocal
@set ExcludeAdminOnly=--where:cat!=AdminOnly
@if "%appveyor%" == "True" set ExcludeAdminOnly=
cd %~dp0
mkdir .\alt
move .\*.pdb .\alt
OpenCover.Console.exe -register:user -target:..\..\..\main\packages\NUnit.ConsoleRunner.3.9.0\tools\nunit3-console.exe -targetargs:"OpenCover.Test.dll --x86 %ExcludeAdminOnly%" -searchdirs:.\alt -excludebyattribute:*.ExcludeFromCoverageAttribute -filter:"+[Open*]* -[OpenCover.T*]* -{nunit-console*}[*]* -{pdb*}[*]*" -output:opencovertests.xml -mergebyhash
move .\alt\*.pdb .
rmdir .\alt
endlocal
popd
