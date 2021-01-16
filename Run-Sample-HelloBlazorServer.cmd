@echo off
dotnet build

rem The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
start cmd /C timeout 3 ^& start http://localhost:5000/"
dotnet run --no-launch-profile -f net5.0 -p src/HelloBlazorServer/HelloBlazorServer.csproj
