@echo off
pushd %cd%
setlocal
cd %~dp0
..\..\..\main\packages\ReportGenerator.4.0.7\tools\net47\ReportGenerator.exe -reports:opencovertests.xml -targetdir:report
endlocal
popd