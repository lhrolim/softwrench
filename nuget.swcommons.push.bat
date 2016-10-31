if [%1]==[] goto blank

cd nugetbuild
del . /F /Q

cd ..

nuget pack softwrench.sw4.batchapi\softwrench.sw4.batches.api.csproj -version %1 -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.user\softwrench.sw4.user.csproj -version %1 -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.problem\softwrench.sw4.problem.csproj -version %1 -build -outputdirectory nugetbuild
nuget pack softwrench.sw4.dashboard\softwrench.sw4.dashboard.csproj -version %1 -build -outputdirectory nugetbuild
nuget pack softwrench.sW4.batches\softwrench.sW4.batches.csproj -version %1 -build -outputdirectory nugetbuild



nuget push nugetbuild\*.nupkg -s http://10.50.100.125:57008

pause
GOTO End1

:blank
  ECHO No version specified
  pause
GOTO End1

:End1
