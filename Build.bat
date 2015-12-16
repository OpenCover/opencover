@echo off
build\nant-0.91-alpha2\bin\nant.exe -f:"%cd%"\default.build %1
@echo.
@echo %date%
@echo %time%
@echo.