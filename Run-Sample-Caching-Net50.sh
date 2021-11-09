#!/bin/bash
docker-compose build db
docker-compose up -d db
dotnet build -c Release

dotnet run --no-launch-profile -c Release -f net5.0 --project src/Caching/Server/Server.csproj &
sleep 3
dotnet run -c Release -f net5.0 --project src/Caching/Client/Client.csproj
