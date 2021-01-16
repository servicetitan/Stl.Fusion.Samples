@echo off
dotnet build

set ASPNETCORE_ENVIRONMENT=Production

set ASPNETCORE_URLS=http://localhost:5005/
start cmd /C timeout 5 ^& start http://localhost:5005/"
start cmd /C dotnet run --no-launch-profile -f net5.0 -p src/Blazor/Server/Server.csproj

set ASPNETCORE_URLS=http://localhost:5006/
start cmd /C timeout 5 ^& start http://localhost:5006/"
start cmd /C dotnet run --no-launch-profile -f net5.0 -p src/Blazor/Server/Server.csproj
