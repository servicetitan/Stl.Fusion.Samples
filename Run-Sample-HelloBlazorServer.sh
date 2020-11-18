#!/bin/bash
dotnet build

# The next line is optional - you need it if you want to debug Blazor client
export ASPNETCORE_ENVIRONMENT=Development
(sleep 3; xdg-open http://localhost:5000/) &
dotnet run -f net5.0 -p src/HelloBlazorServer/HelloBlazorServer.csproj
