@echo off
docker-compose build db
docker-compose up -d db

dotnet build -c Release
start "Samples.Caching.Server" /D "src/Caching/Server" dotnet bin/Release/net5.0/Samples.Caching.Server.dll
timeout 3

dotnet run -c Release -p src/Caching/Client/Client.csproj
