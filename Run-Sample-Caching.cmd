@echo off
docker-compose build db
docker-compose up -d db
dotnet build -c Release

start "Samples.Caching.Server" dotnet run -c Release -f net5.0 -p src/Caching/Server/Server.csproj
timeout 3
dotnet run -c Release -f net5.0 -p src/Caching/Client/Client.csproj
