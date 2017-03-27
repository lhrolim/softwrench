cd nugetbuild
del . /F /Q

cd ..

nuget pack softwrench.sw4.batchapi\softwrench.sw4.batches.api.csproj -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.user\softwrench.sw4.user.csproj -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.problem\softwrench.sw4.problem.csproj -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.dashboard\softwrench.sw4.dashboard.csproj -build -outputdirectory nugetbuild
nuget pack softwrench.sW4.batches\softwrench.sW4.batches.csproj -build -outputdirectory nugetbuild
nuget pack softwrench.sW4.activitystream\softwrench.sW4.activitystream.csproj -build -outputdirectory nugetbuild



nuget push nugetbuild\*.nupkg -s http://10.50.100.125:57008
nuget push nugetbuild\*.nupkg 123 -Source 	http://10.50.100.125:57009/nuget/Default -ApiKey Admin:Admin

pause
