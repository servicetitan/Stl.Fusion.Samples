#!/bin/bash
docker-compose build db
docker-compose up -d db
dotnet build -c Release

export DOTNET_ReadyToRun=0 
export DOTNET_TieredPGO=1 
export DOTNET_TC_QuickJitForLoops=1 

dotnet run --no-launch-profile -c Release -f net6.0 --project src/Caching/Server/Server.csproj &
sleep 3
dotnet run -c Release -f net6.0 --project src/Caching/Client/Client.csproj
