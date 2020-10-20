@echo off
docker-compose build

docker-compose run sample_caching_client dotnet Samples.Caching.Client.dll
