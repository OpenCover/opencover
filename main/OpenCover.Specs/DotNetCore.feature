Feature: DotNetCore
	In order to cut down on the number of .net core related suppport issues
	I want to be able to run OpenCover against applications compiled using supported .net core frameworks

Scenario: Get coverage of a .net core 2.2 application using oldstyle
	Given I can find the OpenCover application
	And I can find the target .net core '2.2' application 'OpenCover.Simple.Target.Core.2.2'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 2.2 application using oldstyle, with debugType = portable 
	Given I can find the OpenCover application
	And I can find the target .net core '2.2' application 'OpenCover.Simple.Target.Core.2.2.Portable'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 2.2 application using oldstyle, with debugType = embedded 
	Given I can find the OpenCover application
	And I can find the target .net core '2.2' application 'OpenCover.Simple.Target.Core.2.2.Embedded'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 3.1 application using oldstyle
	Given I can find the OpenCover application
	And I can find the target .net core '3.1' application 'OpenCover.Simple.Target.Core.3.1'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 3.1 application using oldstyle, with debugType = portable 
	Given I can find the OpenCover application
	And I can find the target .net core '3.1' application 'OpenCover.Simple.Target.Core.3.1.Portable'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 3.1 application using oldstyle, with debugType = embedded 
	Given I can find the OpenCover application
	And I can find the target .net core '3.1' application 'OpenCover.Simple.Target.Core.3.1.Embedded'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application using oldstyle
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application, with debugType = portable 
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0.Portable'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application using oldstyle, with debugType = portable 
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0.Portable'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application, with debugType = embedded 
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0.Embedded'
	When I execute OpenCover against the target application using the switch ''
	Then I should have a results.xml file with a coverage greater than or equal to '100'%

Scenario: Get coverage of a .net core 5.0 application using oldstyle, with debugType = embedded 
	Given I can find the OpenCover application
	And I can find the target .net core '5.0' application 'OpenCover.Simple.Target.Core.5.0.Embedded'
	When I execute OpenCover against the target application using the switch '-oldstyle'
	Then I should have a results.xml file with a coverage greater than or equal to '100'%
