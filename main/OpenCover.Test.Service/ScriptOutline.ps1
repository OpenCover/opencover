
# ACL the executable and .pdb
..\..\..\Grant-NetworkServiceExecuteAccess.ps1 .\OpenCover.Test.Service.exe
..\..\..\Grant-NetworkServiceExecuteAccess.ps1 .\OpenCover.Test.Service.pdb

# And the OpenCover profiler if needed

# Needs to be run as Administrator from here

# register the service
installutil .\OpenCover.Test.Service.exe

# launch DebugView and watch Global Win32
\tools\DebugView.exe

# when that's done, launch OpenCover
start-process .\opencover\OpenCover.console.exe @("-target:OpenCover.Test.Service", "-service:byName", "-output:coverage.xml", "-register")

#after a while stop the service
stop-service "OpenCover.Test.Service"

#tidy up
installutil /u .\OpenCover.Test.Service.exe
