#!/bin/bash
docker-compose build db
docker-compose up -d db
dotnet build -c Release

dotnet run -c Release -f net5.0 -p src/Caching/Server/Server.csproj &
sleep 3
dotnet run -c Release -f net5.0 -p src/Caching/Client/Client.csproj
