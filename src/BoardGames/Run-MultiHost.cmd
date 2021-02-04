@echo off
dotnet build

set ASPNETCORE_ENVIRONMENT=Development

set ASPNETCORE_URLS=http://localhost:5030/
start cmd /C timeout 5 ^& start http://localhost:5030/"
start cmd /C dotnet run --no-launch-profile -f net5.0 -p Host/Host.csproj

set ASPNETCORE_URLS=http://localhost:5031/
start cmd /C timeout 5 ^& start http://localhost:5031/"
start cmd /C dotnet run --no-launch-profile -f net5.0 -p Host/Host.csproj
