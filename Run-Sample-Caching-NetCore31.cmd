@echo off
docker-compose build db
docker-compose up -d db
dotnet build -c Release

start "Samples.Caching.Server" dotnet run --no-launch-profile -c Release -f netcoreapp3.1 -p src/Caching/Server/Server.csproj
timeout 3
dotnet run -c Release -f netcoreapp3.1 -p src/Caching/Client/Client.csproj
