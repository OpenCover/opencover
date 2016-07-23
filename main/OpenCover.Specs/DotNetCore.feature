Feature: DotNetCore
	In order to cut down on the number of .net core related suppport issues
	As a project owner
	I want to be able to run OpenCover against applications compiled using the .net core framework

@mytag
Scenario: Run a .net core application using oldstyle
	Given I can find the OpenCover application
	And I can find the target application
	When I execute OpenCover against the target application using the oldstyle switch
	Then I should have a 'results.xml' file with a coverage greater than or equal to '0'%
