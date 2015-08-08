rem run dogfood tests with shadow
OpenCover.Console.exe -register:user -targetdir:"%cd%" -target:..\..\..\main\packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe -targetargs:"OpenCover.Test.dll /exclude:AdminOnly" -filter:"+[Open*]* -[OpenCover.T*]*" -output:opencovertests.xml
