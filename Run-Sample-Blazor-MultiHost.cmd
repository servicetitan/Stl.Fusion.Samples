@echo off
rem The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development

set ASPNETCORE_URLS=http://localhost:5005/
start cmd /C timeout 5 ^& start http://localhost:5005/"
start cmd /C dotnet run --no-launch-profile --project src/Blazor/Server/Server.csproj

set ASPNETCORE_URLS=http://localhost:5006/
start cmd /C timeout 5 ^& start http://localhost:5006/"
start cmd /C dotnet run --no-launch-profile --project src/Blazor/Server/Server.csproj
