@echo off
docker-compose build

docker-compose run sample_hello_world dotnet Samples.HelloWorld.dll
