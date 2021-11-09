#!/bin/bash
# The next line is optional - you need it if you want to debug Blazor client
export ASPNETCORE_ENVIRONMENT=Development

(sleep 3; xdg-open http://localhost:5005/) &
dotnet run --no-launch-profile --project src/HelloBlazorHybrid/Server/Server.csproj
