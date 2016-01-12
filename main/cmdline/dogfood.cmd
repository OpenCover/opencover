@echo off
pushd %cd%
cd %~dp0
OpenCover.Console.exe -register:user -target:..\..\..\main\packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe -targetargs:"OpenCover.Test.dll /noshadow %ExcludeAdminOnly%" -filter:"+[Open*]* -[OpenCover.T*]* -<nunit-console*>[*]* -<pdb*>[*]*" -output:opencovertests.xml -communicationtimeout:9999
popd