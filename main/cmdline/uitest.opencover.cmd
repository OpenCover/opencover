OpenCover.Console.exe -register:user -target:"%programfiles(x86)%\Microsoft Visual Studio 14.0\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" -targetargs:"OpenCover.UITest.dll" -excludebyattribute:*.ExcludeFromCoverageAttribute -filter:"+[Open*]* -[OpenCover.UIT*]*" -output:opencovertests.xml -mergebyhash -mergeoutput
