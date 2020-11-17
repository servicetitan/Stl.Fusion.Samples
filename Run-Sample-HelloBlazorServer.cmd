@echo off

dotnet build
# The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
start "HelloBlazorServer Sample" dotnet run -f net5.0 -p src/HelloBlazorServer/HelloBlazorServer.csproj
start http://localhost:5000/
