OpenCover.Console.exe -register:user -target:"c:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" -targetargs:"/testcontainer:OpenCover.Test.dll /noisolation" -filter:"+[Open*]* -[OpenCover.T*]*" -mergebyhash -output:opencovertests.xml 
OpenCover.Console.exe -register:user -target:"..\..\..\main\packages\xunit.runners.1.9.2\tools\xunit.console.clr4.x86.exe" -targetargs:"OpenCover.Test.dll /noshadow" -filter:"+[Open*]* -[OpenCover.T*]*" -mergebyhash -output:opencovertests.xml -mergeoutput

