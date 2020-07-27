@echo off

docker-compose build sample_blazor
start "Blazor Sample Server (Docker)" docker-compose up sample_blazor
timeout 3
start http://localhost:5005/
