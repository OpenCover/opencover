@echo off
pushd %cd%
cd %~dp0
REM cover framework only
OpenCover.Console.exe -register:user -target:..\..\..\main\packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe -targetargs:"OpenCover.Test.dll /noshadow %ExcludeAdminOnly%" -filter:"+[Open*]OpenCover.Framework* -[OpenCover.T*]* -{nunit-console*}[*]* -{pdb*}[*]*" -output:opencovertests-framework-filter.xml -communicationtimeout:9999
popd