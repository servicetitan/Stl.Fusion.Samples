@echo off

dotnet build
# The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
start "Blazor Sample Server" dotnet src/Blazor/Server/bin/Debug/netcoreapp3.1/Samples.Blazor.Server.dll
start http://localhost:5005/
