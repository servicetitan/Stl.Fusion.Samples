@echo off

docker-compose build
start "Stl.Fusion Samples - Blazor Server (in Docker)" docker-compose up
timeout 2
start "Stl.Fusion Samples - Blazor Client" http://localhost:5005/
