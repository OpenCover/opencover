rem run dogfood tests with shadow
OpenCover.Console.exe -register:user -targetdir:"%cd%" -target:..\..\..\tools\NUnit-2.6.0.12051\bin\nunit-console-x86.exe -targetargs:"OpenCover.Test.dll" -filter:"+[Open*]* -[OpenCover.T*]*" -output:opencovertests.xml
