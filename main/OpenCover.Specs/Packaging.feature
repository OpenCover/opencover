Feature: Packaging
	In order to avoid deployment issues
	I want to be sure that opencover runs after package installation

@ziptag
Scenario: Run OpenCover after installing from zip file
	Given I have a valid zip package in the output folder
	And I unzip that package into a deployment folder
	When I execute the deployed OpenCover against the x86 target application
	And I execute the deployed OpenCover against the x64 target application
	Then the coverage results should be the same

@nugettag
Scenario: Run OpenCover after installing from a nuget package
	Given I have a valid nugetpackage in the output folder
	And I unpack that package into a deployment folder
	When I execute the deployed OpenCover against the x86 target application, using the tools subfolder
	And I execute the deployed OpenCover against the x64 target application, using the tools subfolder
	Then the coverage results should be the same

@msitag
Scenario: Run OpenCover after installing from an installer
	Given I have a valid installer in the output folder
	And I install that package into a deployment folder
	When I execute the deployed OpenCover against the x86 target application
	And I execute the deployed OpenCover against the x64 target application
	Then the coverage results should be the same
