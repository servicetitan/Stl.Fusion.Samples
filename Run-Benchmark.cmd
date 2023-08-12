@echo off
docker-compose build db
docker-compose up -d db
dotnet run -c Release --project src/Benchmark/Benchmark.csproj -- %*
