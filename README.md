OpenCover is a code coverage tool for .NET 2 and above (Windows OSs only - no MONO), with support for 32 and 64 processes and covers both branch and sequence points. OpenCover was started after attempts to make PartCover support 64-bit processes got just a [little too complicated](http://blog.many-monkeys.com/open_cover_first_beta_release/).

The latest releases can be downloaded from [releases](https://github.com/opencover/opencover/releases). **Alternatively** why not try the [nuget](http://nuget.org/packages/opencover) package (this is the most popular).

| | Active Integrations |
| --- | --- |
| **Build** | [![Build status](https://img.shields.io/appveyor/ci/sawilde/opencover.svg)](https://ci.appveyor.com/project/sawilde/opencover) |
| **Coverage** | <sup>Coveralls</sup> [![Coveralls](https://img.shields.io/coveralls/OpenCover/opencover/master.svg)](https://coveralls.io/r/OpenCover/opencover) | 
| **Nuget** | [![Nuget](https://buildstats.info/nuget/opencover)](http://nuget.org/packages/opencover)  [![Nuget](https://img.shields.io/nuget/vpre/opencover.svg)](http://nuget.org/packages/opencover) |
| **Agreements** | [![CLA assistant](https://cla-assistant.io/readme/badge/OpenCover/opencover)](https://cla-assistant.io/OpenCover/opencover) |

| | Disabled Integrations |
| --- | --- |
| **Coverage** | <sup>SonarCloud</sup> [![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=opencover&metric=coverage)](https://sonarcloud.io/dashboard?id=opencover) | 
| **Quality** | [![SonarCloud](https://sonarcloud.io/api/project_badges/measure?project=opencover&metric=alert_status)](https://sonarcloud.io/dashboard?id=opencover) [![Coverity](https://scan.coverity.com/projects/3921/badge.svg)](https://scan.coverity.com/projects/opencover-opencover) | 

### Master 
The primary repo for the project is [on GitHub](https://github.com/opencover/opencover/) and is also where the [wiki](https://github.com/OpenCover/opencover/wiki) and [issues](https://github.com/OpenCover/opencover/wiki) are managed from.

### Team communication
[![Join the chat at https://gitter.im/OpenCover/opencover](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/OpenCover/opencover?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

### Submissions and Community Licence Agreement
You will be asked to agree to a CLA as part of the submission process; first pull request only and it's all automatic no printing and signing etc. You can read the agreement here - [OpenCover CLA](https://gist.github.com/sawilde/4820db0a6151a1144a0c) and you can read our [Q&A](https://github.com/OpenCover/opencover/wiki/Contributor-License-Agreement-Q&A) on the wiki as to why.

### WIKI
Please review the [wiki pages](https://github.com/opencover/opencover/wiki/_pages) on how to use OpenCover and take particular interest in the [Usage guide](https://github.com/opencover/opencover/wiki/Usage).

### Issues
Please raise issues on GitHub, if you can repeat the issue then please provide a sample to make it easier for us to also repeat it and then implement a fix. Please do not hijack unrelated issues, I would rather you create a new issue than add noise to an unrelated issue.

[Dropbox](http://db.tt/VanqFDn) is very useful for sharing files alternatively you could create a [gist](https://gist.github.com/).

### Licence
All Original Software is licensed under the [MIT Licence](https://github.com/opencover/opencover/blob/master/License.md) and does not apply to any other 3rd party tools, utilities or code which may be used to develop this application.

If anyone is aware of any licence violations that this code may be making please inform the developers so that the issue can be investigated and rectified.

### Integration Test support
OpenCover was initially created to support unit testing techniques such as TDD and tools like NUnit and MSTest. Over time others have found ways to use OpenCover for integration testing especially in tricky scenarios such as IIS and Windows Services. I'll put links here as to how people have achieved this however as they say YMMV (You Mileage May Vary).

#### Mono support
It isn't, sorry - this tool uses the profiler API that is currently only available to .NET Frameworks running on the Windows platform.

#### IIS support
Please refer to the wiki - [IIS Support](https://github.com/OpenCover/opencover/wiki/IIS-Support)

#### DNX support
Please refer to the wiki - [DNX Support](https://github.com/OpenCover/opencover/wiki/DNX-Support)

#### Win8/Win10 Application Support
Not yet [implemented](https://github.com/OpenCover/opencover/issues/144). Sorry no user call/demand for it, I assume the WPF way of separating model and UI has led to people getting all they need from unit-tests; I know how that is how it works for me. 

#### Service support
Please refer to the wiki - [Service Support](https://github.com/OpenCover/opencover/wiki/Service-Support)

#### Silverlight support
Please refer to the wiki - [Silverlight support](https://github.com/OpenCover/opencover/wiki/Silverlight-Support)

### Building
You will need:

1. Visual Studio VS2017 (Community Edition) or later with C#, C++, .Net Core
2. WiX 3.11 or later (http://wixtoolset.org/releases/) and Votive 2017 (https://marketplace.visualstudio.com/items?itemName=RobMensching.WixToolsetVisualStudio2017Extension)
3. SpecFlow for Visual Studio 2017 (https://marketplace.visualstudio.com/items?itemName=TechTalkSpecFlowTeam.SpecFlowforVisualStudio2017)
4. Windows SDK 10 and .NET Framework Tools (https://msdn.microsoft.com/en-us/windows/desktop/bg162891.aspx)
5. Microsoft .NET Framework 4.7.1 Developer Pack (https://www.microsoft.com/en-us/download/details.aspx?id=56119)

All other software should be included with this repository. 

NANT scripts (encapsulated by the build.bat file) are used to build the project outside visual studio and will run all unit tests.

To build the code in 32-bit Debug mode just run Build in the root of the project folder.

To build a release package including installer, zip and nuget packages use 

> build create-release

### Build Server
The OpenCover team is using [Appveyor](http://www.appveyor.com/) for automated building. It runs a staged equivalent of

> build create-release

and stores all the information about tests, coverage and other build artefacts; you can use these [builds](https://ci.appveyor.com/project/sawilde/opencover/build/artifacts) if you want to be 'cutting' edge but support will be limited.

We have also suppied the [appveyor.yml](appveyor.yml) to make it easy for any forks to have their own builds on AppVeyor. If you send a pull-request the CI system will test your commits (which saves the team time if we do it ourselves)

### Coverage
The current OpenCover coverage (found by [dogfooding](http://en.wikipedia.org/wiki/Eating_your_own_dog_food) OpenCover on its own tests) can be viewed here

[Code Coverage via Coveralls](https://coveralls.io/r/OpenCover/opencover)

### Reports
For local viewing of the output from OpenCover [start here.](https://github.com/opencover/opencover/wiki/Reports)

### Latest Drop as ZIP
No Git? Don't worry you can download the latest code as a [zip file](http://github.com/opencover/opencover/zipball/master).

### Thanks
I would like to thank 

* JetBrains for my Open Source [ReSharper licence](http://www.jetbrains.com/resharper/),
<img src="http://www.jetbrains.com/company/docs/logo_jetbrains.png"/>

* [AppVeyor](https://ci.appveyor.com/project/sawilde/opencover) for allowing free build CI services for Open Source projects,
* [Coveralls](https://coveralls.io/r/OpenCover/opencover) for allowing free services for Open Source projects,
* NDepend for my [NDepend licence](http://www.ndepend.com/),
* the guys at [CodeBetter](http://codebetter.com/), [Devlicious](http://devlicio.us/) and [Los Techies](http://lostechies.com/) who orignally arranged my MSDN licence all those years ago without which I doubt I'd have been able to start OpenCover (now no longer needed as we can build OpenCover using the VS2013 Community Edition), 
* the [NextGenUG](http://www.nxtgenug.net/) and their free swag from where I got lots of useful tools,


I'd also like to thank my family, employers, colleagues and friends for all their support. 
