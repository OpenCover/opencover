@ECHO OFF

:: Reset ERRORLEVEL
VERIFY OTHER 2>nul
SETLOCAL ENABLEEXTENSIONS ENABLEDELAYEDEXPANSION
IF ERRORLEVEL 1 GOTO ERROR_EXT


SET PROJECT=DogFood.proj
SET TARGET=Default
SET VERBOSITY=detailed
GOTO SETENV

:BUILD
msbuild.exe %PROJECT% /nologo /t:%TARGET% /m:%NUMBER_OF_PROCESSORS% /l:FileLogger,Microsoft.Build.Engine;logfile=dogfood_msbuild.log;verbosity=%VERBOSITY%;encoding=UTF-8 /nr:False
GOTO END


:SETENV
CALL :SetMSBuildToolsPathHelper > nul 2>&1
IF ERRORLEVEL 1 GOTO ERROR_MSBUILD

SET PATH=%MSBuildToolsPath%;%PATH%
GOTO BUILD


:SetMSBuildToolsPathHelper
SET MSBuildToolsPath=
FOR /F "tokens=1,2*" %%i in ('REG QUERY HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\MSBuild\ToolsVersions\4.0 /V MSBuildToolsPath') DO (
    IF "%%i"=="MSBuildToolsPath" (
        SET "MSBuildToolsPath=%%k"
    )
)
IF "%MSBuildToolsPath%"=="" EXIT /B 1
EXIT /B 0

:ERROR_EXT
ECHO Could not activate command extensions
GOTO END

:ERROR_MSBUILD
ECHO Could not find MSBuild 4.0
GOTO END

:END
ENDLOCAL
