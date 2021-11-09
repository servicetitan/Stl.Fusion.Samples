@echo off
rem The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development

start cmd /C timeout 3 ^& start http://localhost:5005/"
dotnet run --no-launch-profile --project src/HelloBlazorServer/HelloBlazorServer.csproj
