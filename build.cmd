set artifacts=%~dp0artifacts
set /p ver=<VERSION

if exist %artifacts%  rd /q /s %artifacts%

call dotnet pack src/Es.Throttle -c release -p:Ver=%ver% -o %artifacts%
call dotnet pack src/Es.Throttle.Mvc -c release -p:Ver=%ver% -o %artifacts%

pause