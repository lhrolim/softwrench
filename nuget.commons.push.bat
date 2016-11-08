cd nugetbuild
del . /F /Q

cd ..

nuget pack cts.commons\cts.commons.csproj -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.simpleinjector\cts.commons.simpleinjector.csproj -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.persistence\cts.commons.persistence.csproj  -build -outputdirectory nugetbuild -Symbols
nuget pack cts.commons.web\cts.commons.web.csproj  -build -outputdirectory nugetbuild -Symbols

nuget push nugetbuild\*.nupkg -s http://10.50.100.125:57008
nuget push nugetbuild\*.nupkg 123 -Source 	http://10.50.100.125:57009/nuget/Default -ApiKey Admin:Admin

pause

