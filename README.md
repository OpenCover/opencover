The latest releases can be downloaded from [releases](https://github.com/opencover/opencover/releases) or from the OpenCover mirror site on [bitbucket](https://bitbucket.org/shaunwilde/opencover/downloads).

### Master 

The primary repo for the project is [over here](https://github.com/opencover/opencover/).
Alternatively why not try the [nuget](http://nuget.org/packages/opencover) package (this is the most popular).

### Licence
All Original Software is licensed under the [MIT Licence](https://github.com/opencover/opencover/blob/master/License.md) and does not apply to any other 3rd party tools, utilities or code which may be used to develop this application.

If anyone is aware of any licence violations that this code may be making please inform the developers so that the issue can be investigated and rectified.

### WIKI
Please review the [wiki pages](https://github.com/opencover/opencover/wiki/_pages) on how to use OpenCover and take particular interest in the [Usage guide](https://github.com/opencover/opencover/wiki/Usage).

### Service support

Note that .zip and .nuget packages (at least) will not ACL files so that a low-privilege service can execute them -- manually add the service principal with read/execute privilege to the OpenCover.Profiler.dll ACLs, either through explorer properties or by a `Get-Acl`/`Set-Acl` PowerShell script.

Launching the profiling operation must be done with Administrator privilege -- this is required to even start the service, and is also needed to create the global objects and set the per-service environment that registers the COM object.  Assuming that the COM objects have been pre-registered, the command line to use is like

    OpenCover.console.exe -service:byname -target:<service name> -output:coverage.xml
    
As this is a blocking operation, launching the coverage gathering a separate process as 

    start-process OpenCover.console.exe @("-target:<service name>", "-service:byName", "-output:<service name>coverage.xml")
    
or equivalent in your test scripts.

The `byname` qualifier is used to prevent races if multiple services are being started concurrently by injecting the OpenCover profiling information to the service specific environment.

### Issues
Please raise issues on GitHub, if you can repeat the issue then please provide a sample to make it easier for us to also repeat it and then implement a fix. Please do not hijack unrelated issues, I would rather you create a new issue than add noise to an unrelated issue.

Dropbox is very useful for sharing files [Dropbox](http://db.tt/VanqFDn)

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

### Build Server
The OpenCover team is trying out using [Atlassian Bamboo](https://opencover.atlassian.net/builds/telemetry.action) for automated building. It runs a staged equivalent of

> build create-release

and stores all the information about tests, coverage and other build artefacts; you can use these builds if you want to be 'cutting' edge but support will be limited.

### Coverage
The current OpenCover coverage (found by [dogfooding](http://en.wikipedia.org/wiki/Eating_your_own_dog_food) OpenCover on its own tests) can be viewed here

http://opencover-metrics.herokuapp.com/charts/sample_chart.htm

The nodejs application used to store and display the results can also be found on Github - https://github.com/sawilde/metrics-store

### Reports
For viewing the output from OpenCover [start here.](https://github.com/opencover/opencover/wiki/Reports)

### Latest Drop as ZIP
No Git? Don't worry you can download the latest code as a [zip file](http://github.com/opencover/opencover/zipball/master).

### Project Management
Was using AgileZen (which I quite liked) but the maintenance of an online board (for 1 person, me) vs the card wall (in office) got monotonous for no real gain.

### Thanks
I would like to thank 

* the guys at [CodeBetter](http://codebetter.com/), [Devlicious](http://devlicio.us/) and [Los Techies](http://lostechies.com/) who arranged my MSDN licence, 
* the [NextGenUG](http://www.nxtgenug.net/) and their free swag from where I got lots of useful tools,
* JetBrains for my Open Source [ReSharper licence](http://www.jetbrains.com/resharper/),
* Atlassian for my Open Source [Bamboo licence](https://www.atlassian.com/software/bamboo),
* NDepend for my [NDepend licence](http://www.ndepend.com/).

I'd also like to thank my employers, colleagues and friends for all their support. 
