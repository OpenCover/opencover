Feature: DotNetCore
	In order to cut down on the number of .net core related suppport issues
	As a project owner
	I want to be able to run OpenCover against applications compiled using the .net core framework

Scenario: Get coverage of a .net core application using oldstyle
	Given I can find the OpenCover application
	And I can find the target .net core application 'OpenCover.Simple.Target.Core'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core application 
	Given I can find the OpenCover application
	And I can find the target .net core application 'OpenCover.Simple.Target.Core'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core application with debugType = portable 
	Given I can find the OpenCover application
	And I can find the target .net core portable application 'OpenCover.Simple.Target.Core.Portable'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core application with debugType = embedded 
	Given I can find the OpenCover application
	And I can find the target .net core portable application 'OpenCover.Simple.Target.Core.Embedded'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%
