@echo off
set /p ver=<VERSION
set sourceUrl=-s https://www.nuget.org/api/v2/package

dotnet nuget push artifacts/Es.Throttle.%ver%.nupkg  %sourceUrl%
dotnet nuget push artifacts/Es.Throttle.Mvc.%ver%.nupkg  %sourceUrl%

pause