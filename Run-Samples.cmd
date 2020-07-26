@echo off

dotnet build
# The next line is optional - you need it if you want to debug Blazor client
set ASPNETCORE_ENVIRONMENT=Development
start "Stl.Fusion Samples - Blazor Server" dotnet src/StlFusionSamples.Blazor.Server/StlFusionSamples.Blazor.Server.dll
start "Stl.Fusion Samples - Blazor Client" http://localhost:5005/
