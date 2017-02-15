if [%1]==[] goto blank

cd nugetbuild
del . /F /Q

cd ..

nuget pack cts.commons\cts.commons.csproj -version %1 -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.simpleinjector\cts.commons.simpleinjector.csproj -version %1 -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.persistence\cts.commons.persistence.csproj -version %1 -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.web\cts.commons.web.csproj -version %1 -build -outputdirectory nugetbuild -Symbols

nuget push *.nupkg -s http://10.50.100.125:57008
nuget push *.nupkg 123 -Source 	http://10.50.100.125:57009/nuget/Default -ApiKey Admin:Admin

pause
GOTO End1

:blank
  ECHO No version specified
GOTO End1

:End1
