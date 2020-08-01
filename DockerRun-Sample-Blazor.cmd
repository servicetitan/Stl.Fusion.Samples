@echo off

docker-compose build sample_blazor
start "Blazor Sample (Docker)" docker-compose up sample_blazor
timeout 3
start http://localhost:5005/
