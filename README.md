[![endorse](http://api.coderwall.com/sawilde/endorsecount.png)](http://coderwall.com/sawilde) 

### Downloads

The latest releases can be downloaded from [releases](https://github.com/sawilde/opencover/releases) or from the OpenCover mirror site on [bitbucket](https://bitbucket.org/shaunwilde/opencover/downloads).

Alternatively why not try the [nuget](http://nuget.org/packages/opencover) package (this is the most popular).

### Licence
All Original Software is licensed under the [MIT Licence](https://github.com/sawilde/opencover/blob/master/License.md) and does not apply to any other 3rd party tools, utilities or code which may be used to develop this application.

If anyone is aware of any licence violations that this code may be making please inform the developers so that the issue can be investigated and rectified.

### WIKI
Please review the [wiki pages](https://github.com/OpenCover/opencover/wiki/_pages) on how to use OpenCover and take particular interest in the [Usage guide](https://github.com/OpenCover/opencover/wiki/Usage).

### Issues
Please raise issues on GitHub, if you can repeat the issue then please provide a sample to make it easier for us to also repeat it and then implement a fix. Please do not hijack unrelated issues, I would rather you create a new issue than add noise to an unrelated issue.

Dropbox is very useful for sharing files [Dropbox](http://db.tt/VanqFDn)

### Building
You will need:

1. Visual Studio VS2012 with C# and C++
2. WiX 3.7 (http://wix.codeplex.com/releases/view/99514)
3. Specflow (http://visualstudiogallery.msdn.microsoft.com/9915524d-7fb0-43c3-bb3c-a8a14fbd40ee)

All other software should be included with this repository. 

NANT scripts (encapsulated by the build.bat file) are used to build the project outside visual studio and will run all unit tests.

To build the code in 32-bit Debug mode just run Build in the root of the project folder.

To build a release package including installer, zip and nuget packages use 

> build create-release

### Build Server
The OpenCover team is trying out using [Atlassian Bamboo](https://opencover.atlassian.net/builds/browse/OPC-DEF/latest) for automated building. It runs a staged equivalent of

> build create-release

and stores all the information about tests, coverage and other build artefacts; you can use these builds if you want to be 'cutting' edge but support will be limited.

### Coverage
The current OpenCover coverage (found by [dogfooding](http://en.wikipedia.org/wiki/Eating_your_own_dog_food) OpenCover on its own tests) can be viewed here

http://opencover-metrics.herokuapp.com/charts/sample_chart.htm

The nodejs application used to store and display the results can also be found on Github - https://github.com/sawilde/metrics-store

### Reports
For viewing the output from OpenCover [start here.](https://github.com/sawilde/opencover/wiki/Reports)

### Latest Drop as ZIP
No Git? Don't worry you can download the latest code as a [zip file](http://github.com/sawilde/opencover/zipball/master).

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

