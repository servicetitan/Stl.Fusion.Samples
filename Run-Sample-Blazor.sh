#!/bin/bash
dotnet build

# The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
(sleep 3; xdg-open http://localhost:5005/) &
dotnet run --no-launch-profile -f net5.0 -p src/Blazor/Server/Server.csproj
