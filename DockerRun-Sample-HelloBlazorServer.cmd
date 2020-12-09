@echo off
start cmd /C timeout 3 ^& start http://localhost:5000/"
docker-compose run --service-ports sample_hello_blazor_server
