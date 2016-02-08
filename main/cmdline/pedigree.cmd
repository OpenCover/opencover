@echo off
pushd %cd%
cd %~dp0
OpenCover.Console.exe -register:user -enableperformancecounters -target:dogfood.cmd -filter:+[OpenCover*]* -output:pedigree_results.xml
popd