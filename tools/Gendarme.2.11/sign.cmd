md Temp
del Signed\*.* /q
ildasm.exe .\Gendarme.Framework.dll /out:.\Temp\Gendarme.Framework.il
ilasm.exe .\Temp\Gendarme.Framework.il /dll /key=..\..\build\version\opencover.snk /output=.\Signed\Gendarme.Framework.dll
del Temp\*.* /q
ildasm.exe .\Gendarme.Rules.Maintainability.dll /out:.\Temp\Gendarme.Rules.Maintainability.il
echo now is a good time to manually edit that public key in Temp\Gendarme.Rules.Maintainability.il - .publickeytoken = (28 36 AC 38 85 2A 8f 79 )
pause
ilasm.exe .\Temp\Gendarme.Rules.Maintainability.il /dll /key=..\..\build\version\opencover.snk /output=.\Signed\Gendarme.Rules.Maintainability.dll
del Temp\*.* /q
rd Temp
