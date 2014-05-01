### Service support

Note that .zip and .nuget packages (at least) will not ACL files so that a low-privilege service can execute them -- manually add the service principal with read/execute privilege to the OpenCover.Profiler.dll ACLs, either through explorer properties or by a `Get-Acl`/`Set-Acl` PowerShell script.

Launching the profiling operation must be done with Administrator privilege -- this is required to even start the service, and is also needed to create the global objects and set the per-service environment that registers the COM object.  Assuming that the COM objects have been pre-registered, the command line to use is like

    OpenCover.console.exe -service:byname -target:<service name> -output:coverage.xml
    
As this is a blocking operation, launching the coverage gathering a separate process as 

    start-process OpenCover.console.exe @("-target:<service name>", "-service:byName", "-output:<service name>coverage.xml")
    
or equivalent in your test scripts.

The `byname` qualifier is used to prevent races if multiple services are being started concurrently by injecting the OpenCover profiling information to the service specific environment.

### Master 

The primary repo for the project is [over here](https://github.com/sawilde/opencover/).
Alternatively why not try the [nuget](http://nuget.org/packages/opencover) package (this is the most popular).

### Licence
All Original Software is licensed under the [MIT Licence](https://github.com/sawilde/opencover/blob/master/License.md) and does not apply to any other 3rd party tools, utilities or code which may be used to develop this application.

If anyone is aware of any licence violations that this code may be making please inform the developers so that the issue can be investigated and rectified.

### Building
You will need:

1. Visual Studio VS2012 or later with C# and C++
2. WiX 3.7 or later (http://wix.codeplex.com/releases/view/99514)
3. Specflow (http://visualstudiogallery.msdn.microsoft.com/9915524d-7fb0-43c3-bb3c-a8a14fbd40ee)

All other software should be included with this repository. 

NANT scripts (encapsulated by the build.bat file) are used to build the project outside visual studio and will run all unit tests.

To build the code in 32-bit Debug mode just run Build in the root of the project folder.

To build a release package including installer, zip and nuget packages use 

> build create-release