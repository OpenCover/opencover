OpenCover.Console.exe -register:user -target:"c:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\IDE\MSTest.exe" -targetargs:"/testcontainer:OpenCover.Test.dll /noisolation" -filter:"+[Open*]* -[OpenCover.T*]*" -mergebyhash -output:opencovertests.xml 
OpenCover.Console.exe -register:user -target:"..\..\..\main\packages\xunit.runner.console.2.1.0\tools\xunit.console.x86.exe" -targetargs:"OpenCover.Test.dll -noshadow" -filter:"+[Open*]* -[OpenCover.T*]*" -mergebyhash -output:opencovertests.xml -mergeoutput

