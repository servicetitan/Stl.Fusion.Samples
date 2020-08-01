@echo off

dotnet build
# The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
start "HelloBlazorServer Sample" /D "src/HelloBlazorServer" dotnet bin/Debug/netcoreapp3.1/Samples.HelloBlazorServer.dll
start http://localhost:5000/
