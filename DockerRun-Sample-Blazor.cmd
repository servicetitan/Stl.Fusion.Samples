@echo off
docker-compose build

start "Samples.Blazor.Server (Docker)" docker-compose up sample_blazor
timeout 3
start http://localhost:5005/
