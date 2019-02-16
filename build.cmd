set artifacts=%~dp0artifacts

if exist %artifacts%  rd /q /s %artifacts%

call dotnet restore src/Es.Throttle
call dotnet restore src/Es.Throttle.Mvc

call dotnet build src/Es.Throttle -f netstandard1.3 -c Release -o %artifacts%\netstandard1.3
call dotnet build src/Es.Throttle -f netstandard2.0 -c Release -o %artifacts%\netstandard2.0
call dotnet build src/Es.Throttle.Mvc -f netstandard1.6 -c Release -o %artifacts%\netstandard1.6
call dotnet build src/Es.Throttle.Mvc -f netstandard2.0 -c Release -o %artifacts%\netstandard2.0

call dotnet build src/Es.Throttle -f net46 -c Release -o %artifacts%\net46

call dotnet pack src/Es.Throttle -c release -o %artifacts%
call dotnet pack src/Es.Throttle.Mvc -c release -o %artifacts%

pause